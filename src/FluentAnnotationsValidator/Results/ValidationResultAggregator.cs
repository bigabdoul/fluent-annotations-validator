using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Provides a set of static methods for aggregating validation errors from multiple rules
/// into a unified result set.
/// </summary>
public static class ValidationResultAggregator
{
    private static readonly ConcurrentDictionary<Type, List<IValidationRule>> _cachedElementRules = new();

    /// <summary>
    /// Synchronously validates all rules for a given member and returns error results.
    /// This method evaluates conditional rules and executes attribute-based validations
    /// on the member's value.
    /// </summary>
    /// <param name="rules">The conditional rules to evaluate.</param>
    /// <param name="instance">The model instance containing the member.</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the property or field.</param>
    /// <param name="resolver">An object used for validation message resolution.</param>
    /// <param name="ruleRegistry">An object providing access to the rule registry.</param>
    /// <param name="validationContextItems">A dictionary of key/value pairs to associate with the validation context.</param>
    /// 
    /// <returns>A list of <see cref="ValidationErrorResult"/> if any rules failed; an empty list otherwise.</returns>
    public static List<ValidationErrorResult> Validate(
    this IEnumerable<IValidationRule> rules,
    object instance,
    MemberInfo member,
    IValidationMessageResolver resolver,
    IRuleRegistry ruleRegistry,
    IDictionary<object, object?>? validationContextItems = null)
    {
        var ruleList = rules as IList<IValidationRule> ?? [.. rules];
        if (ruleList.Count == 0)
            return [];

        var rulesToValidate = new Dictionary<IValidationRule, bool>();
        var errors = new List<ValidationErrorResult>();
        var value = member.GetValue(instance);
        var preconfigurationInvoked = false;
        ValidationContext? context = null;

        // Evaluate fluent rules once
        foreach (var rule in ruleList)
        {
            if (!rule.HasValidator)
            {
                var shouldValidate = rule.ShouldValidate(instance);
                rulesToValidate[rule] = shouldValidate;
            }
        }

        // Skip if no fluent rule applies
        if (rulesToValidate.Count > 0 && !rulesToValidate.Values.Any(v => v))
            return [];

        foreach (var rule in ruleList)
        {
            var shouldSkip = rule.Validator is null ||
               (rulesToValidate.TryGetValue(rule, out var shouldValidate) && !shouldValidate) ||
               (!rulesToValidate.ContainsKey(rule) && !rule.ShouldValidate(instance));

            if (shouldSkip)
                continue;

            if (!preconfigurationInvoked)
            {
                GetPrevalidationValue(ruleList, instance, member, ref value, ref preconfigurationInvoked);
            }

            switch (rule.Validator)
            {
                case FluentRuleAttribute fluentRule:
                    var fluentRules = ruleRegistry.GetRulesForType(fluentRule.ObjectType);
                    var fluentErrors = fluentRules.Validate(instance, member, resolver, ruleRegistry, validationContextItems);
                    errors.AddRange(fluentErrors);
                    break;

                case ValidationAttribute attr:
                    if (context is null)
                    {
                        context = new ValidationContext(instance) { MemberName = member.Name };
                        MergeItems(context.Items, validationContextItems);
                    }

                    var isUnresolvedMessage = CheckFluentValidationAttribute(attr, rule, resolver);
                    var result = attr.GetValidationResult(value, context);

                    if (result != ValidationResult.Success)
                    {
                        AddErrors(errors, instance, member, attr, value, rule, result, resolver, isUnresolvedMessage);
                    }
                    break;
            }
        }

        return errors;
    }

    /// <summary>
    /// Asynchronously validates all rules for a given member and returns error results.
    /// This method evaluates conditional rules and executes attribute-based validations
    /// on the member's value in a non-blocking manner.
    /// </summary>
    /// <param name="rules">The conditional rules to evaluate.</param>
    /// <param name="instance">The model instance containing the member.</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the property or field.</param>
    /// <param name="resolver">An object used for validation message resolution.</param>
    /// <param name="ruleRegistry">An object providing access to the rule registry.</param>
    /// <param name="validationContextItems">A dictionary of key/value pairs to associate with the validation context.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a list of <see cref="ValidationErrorResult"/> if any rules failed;
    /// an empty list otherwise.
    /// </returns>
    public static async Task<List<ValidationErrorResult>> ValidateAsync(
    this IEnumerable<IValidationRule> rules,
    object instance,
    MemberInfo member,
    IValidationMessageResolver resolver,
    IRuleRegistry ruleRegistry,
    IDictionary<object, object?>? validationContextItems = null,
    CancellationToken cancellationToken = default)
    {
        var ruleList = rules as IList<IValidationRule> ?? rules.ToList();
        if (ruleList.Count == 0)
            return [];

        var rulesToValidate = new Dictionary<IValidationRule, bool>();
        var errors = new List<ValidationErrorResult>();
        var value = member.GetValue(instance);
        var preconfigurationInvoked = false;
        ValidationContext? context = null;

        // Evaluate fluent rules once
        var fluentRules = ruleList.Where(r => !r.HasValidator).ToList();
        if (fluentRules.Count > 0)
        {
            var conditionTasks = fluentRules
                .Select(r => r.ShouldValidateAsync(instance, cancellationToken));

            var conditionResults = await Task.WhenAll(conditionTasks);

            bool anyApplies = false;
            for (int i = 0; i < fluentRules.Count; i++)
            {
                rulesToValidate[fluentRules[i]] = conditionResults[i];
                anyApplies |= conditionResults[i];
            }

            if (!anyApplies)
                return [];
        }

        foreach (var rule in ruleList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool shouldSkip =
                rule.Validator is null ||
                (rulesToValidate.TryGetValue(rule, out var cached) && !cached) ||
                (!rulesToValidate.ContainsKey(rule) && !await rule.ShouldValidateAsync(instance, cancellationToken));

            if (shouldSkip)
                continue;

            if (!preconfigurationInvoked)
            {
                GetPrevalidationValue(ruleList, instance, member, ref value, ref preconfigurationInvoked);
            }

            switch (rule.Validator)
            {
                case FluentRuleAttribute fluentRule:
                    var nestedRules = ruleRegistry.GetRulesForType(fluentRule.ObjectType);
                    var nestedErrors = await nestedRules.ValidateAsync(instance, member, resolver, ruleRegistry, validationContextItems, cancellationToken);
                    errors.AddRange(nestedErrors);
                    break;

                case ValidationAttribute attr:
                    if (context is null)
                    {
                        context = new ValidationContext(instance) { MemberName = member.Name };
                        MergeItems(context.Items, validationContextItems);
                    }

                    var isUnresolvedMessage = CheckFluentValidationAttribute(attr, rule, resolver);

                    var result = attr is IAsyncValidationAttribute asyncAttr
                        ? await asyncAttr.ValidateAsync(value, context, cancellationToken)
                        : attr.GetValidationResult(value, context);

                    if (result != ValidationResult.Success)
                    {
                        AddErrors(errors, instance, member, attr, value, rule, result, resolver, isUnresolvedMessage);
                    }
                    break;
            }
        }

        return errors;
    }

    private static void AddErrors(List<ValidationErrorResult> errors,
    object instance, MemberInfo member, ValidationAttribute attr, object? value, IValidationRule rule,
    ValidationResult? result, IValidationMessageResolver resolver, bool isUnresolvedMessage)
    {
        var propertyName = rule.GetPropertyName();

        if (attr is ICollectionValidationResult collectionValidationResult)
        {
            // Convert the errors to instances of ValidationErrorResult
            // containing the FluentValidationFailure errors collection.
            // This way, the Validate(...) extension method's caller may inspect 
            // the presence of the 'Failure' property and use that directly.
            // The choice of this pattern is justified by the return value
            // of the extension method, which contrasts with the type of
            // the ICollectionValidationResult.Errors property items.

            // One might wonder: Why not simply use ValidationErrorResult
            // (since it shares similar properties with FluentValidationFailure)?
            // FluentValidationFailure is marked as [Serializable], and 
            // ValidationErrorResult is not, because it contains non-serializable
            // properties like 'Member' (MemberInfo type) and 'Attribute' (ValidationAttribute type).
            errors.AddRange(collectionValidationResult.Errors.Select(err => new ValidationErrorResult(err)
            {
                PropertyName = propertyName
            }));
        }
        else
        {
            var message = isUnresolvedMessage
                ? resolver.ResolveMessage(instance, propertyName, attr, rule: rule)
                : result?.ErrorMessage;

            errors.Add(new ValidationErrorResult
            {
                AttemptedValue = value,
                Member = member,
                PropertyName = propertyName,
                Message = message ?? result?.ErrorMessage ?? $"Validation failed for {propertyName}.",
                UniqueKey = rule.UniqueKey,
                Attribute = attr,
            });
        }
    }

    /// <summary>
    /// Gets and applies the value from a pre-validation configuration, if one is defined.
    /// This method is designed to be invoked only once per member validation.
    /// </summary>
    /// <param name="rules">The collection of rules to check for a pre-validation delegate.</param>
    /// <param name="instance">The model instance being validated.</param>
    /// <param name="member">The member being validated.</param>
    /// <param name="value">A reference to the member's value, which may be updated by the pre-validation delegate.</param>
    /// <param name="preconfigurationInvoked">
    /// A flag that is set to <see langword="true"/> after the pre-validation delegate has been invoked.
    /// </param>
    private static void GetPrevalidationValue(IEnumerable<IValidationRule> rules,
        object instance,
        MemberInfo member,
        ref object? value,
        ref bool preconfigurationInvoked)
    {
        var ruleWithPrevalidation = rules.FirstOrDefault(r => r.ConfigureBeforeValidation != null);

        if (ruleWithPrevalidation != null)
        {
            var newValue = ruleWithPrevalidation.ConfigureBeforeValidation!.Invoke(instance, member, value);
            if (newValue != value)
            {
                value = newValue;
                // Make sure the user didn't forget to update the member with the new value.
                if (member.GetValue(instance) != newValue)
                {
                    // try to synchronize the member's value
                    member.TrySetValue(instance, newValue);
                }
            }
        }

        preconfigurationInvoked = true;
    }

    private static void MergeItems(IDictionary<object, object?> contextItems, IDictionary<object, object?>? items)
    {
        if (items != null)
        {
            foreach (var item in items)
                contextItems[item.Key] = item.Value;
        }
    }

    private static bool CheckFluentValidationAttribute(ValidationAttribute attr, IValidationRule rule,
    IValidationMessageResolver resolver)
    {
        bool isUnresolvedMessage = true; // Indicates whether to consider the error message unresolved.
        if (attr is FluentValidationAttribute fva)
        {
            fva.Rule = rule;
            fva.MessageResolver ??= resolver;
            // Since the attribute has a set resolver, it will use that one should validation fail.
            isUnresolvedMessage = resolver == null;
        }
        return isUnresolvedMessage;
    }
}
