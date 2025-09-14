using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
    /// <param name="contextItems">A dictionary of key/value pairs to associate with the validation context.</param>
    /// 
    /// <returns>A list of <see cref="ValidationErrorResult"/> if any rules failed; an empty list otherwise.</returns>
    public static List<ValidationErrorResult> Validate(
    this IEnumerable<IValidationRule> rules,
    object instance,
    MemberInfo member,
    IValidationMessageResolver resolver,
    IRuleRegistry ruleRegistry,
    IDictionary<object, object?>? contextItems = null)
    {
        var ruleList = rules as IList<IValidationRule> ?? [.. rules];
        if (ruleList.Count == 0)
            return [];

        var rulesToValidate = new Dictionary<IValidationRule, bool>();
        var errors = new List<ValidationErrorResult>();
        var memberValue = member.GetValue(instance);
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
                GetPrevalidationValue(ruleList, instance, member, ref memberValue, ref preconfigurationInvoked);
            }

            switch (rule.Validator)
            {
                case FluentRuleAttribute fluentAttr:
                    ProcessFluentRule(fluentAttr, instance, member, memberValue, contextItems, rule, resolver, ruleRegistry, errors);
                    break;
                case ValidationAttribute attr:
                    context ??= new ValidationContext(instance, contextItems) { MemberName = member.Name };
                    var isUnresolvedMessage = CheckFluentValidationAttribute(attr, rule, resolver, ruleRegistry);
                    var result = attr.GetValidationResult(memberValue, context);

                    if (result != ValidationResult.Success)
                    {
                        AddErrors(errors, instance, member, attr, memberValue, rule, result, resolver, isUnresolvedMessage);
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
    /// <param name="contextItems">A dictionary of key/value pairs to associate with the validation context.</param>
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
    IDictionary<object, object?>? contextItems = null,
    CancellationToken cancellationToken = default)
    {
        var ruleList = rules as IList<IValidationRule> ?? [.. rules];
        if (ruleList.Count == 0)
            return [];

        var rulesToValidate = new Dictionary<IValidationRule, bool>();
        var errors = new List<ValidationErrorResult>();
        var memberValue = member.GetValue(instance);
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
                GetPrevalidationValue(ruleList, instance, member, ref memberValue, ref preconfigurationInvoked);
            }

            switch (rule.Validator)
            {
                case FluentRuleAttribute fluentAttr:
                    await ProcessFluentRuleAsync(fluentAttr, instance, member, memberValue, contextItems, rule, resolver, ruleRegistry, errors, cancellationToken);
                    break;
                case ValidationAttribute attr:
                    context ??= new ValidationContext(instance, contextItems) { MemberName = member.Name };
                    var isUnresolvedMessage = CheckFluentValidationAttribute(attr, rule, resolver, ruleRegistry);

                    var result = attr is IAsyncValidationAttribute asyncAttr
                        ? await asyncAttr.ValidateAsync(memberValue, context, cancellationToken)
                        : attr.GetValidationResult(memberValue, context);

                    if (result != ValidationResult.Success)
                    {
                        AddErrors(errors, instance, member, attr, memberValue, rule, result, resolver, isUnresolvedMessage);
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

        if (attr is IValidationResult validationResult && validationResult.Errors.Count > 0)
        {
            // Convert the errors to instances of ValidationErrorResult
            // containing the FluentValidationFailure errors collection.
            // This way, the Validate(...) extension method's caller may inspect 
            // the presence of the 'Failure' property and use that directly.
            // The choice of this pattern is justified by the return value
            // of the extension method, which contrasts with the type of
            // the IValidationResult.Errors property items.

            // One might wonder: Why not simply use ValidationErrorResult
            // (since it shares similar properties with FluentValidationFailure)?
            // FluentValidationFailure is marked as [Serializable], and 
            // ValidationErrorResult is not, because it contains non-serializable
            // properties like 'Member' (MemberInfo type) and 'Attribute' (ValidationAttribute type).
            errors.AddRange(validationResult.Errors.Select(err => new ValidationErrorResult(err)
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

    private static bool CheckFluentValidationAttribute(ValidationAttribute attr, IValidationRule rule,
    IValidationMessageResolver resolver, IRuleRegistry registry)
    {
        bool isUnresolvedMessage = true; // Indicates whether to consider the error message unresolved.
        if (attr is FluentValidationAttribute fva)
        {
            fva.Rule = rule;
            fva.MessageResolver ??= resolver;
            fva.RuleRegistry ??= registry;
            // Since the attribute has a set resolver, it will use that one should validation fail.
            isUnresolvedMessage = resolver == null;
        }
        return isUnresolvedMessage;
    }

    private static void ProcessFluentRule(FluentRuleAttribute attribute,
    object instance, MemberInfo member, object? memberValue,
    IDictionary<object, object?>? contextItems,
    IValidationRule rule, IValidationMessageResolver resolver, 
    IRuleRegistry registry, List<ValidationErrorResult> errors)
    {
        var isUnresolvedMessage = CheckFluentValidationAttribute(attribute, rule, resolver, registry);
        var context = new ValidationContext(instance, contextItems) { MemberName = member.Name };
        var fluentResult = attribute.GetValidationResult(instance, context);
        if (fluentResult != ValidationResult.Success)
        {
            AddErrors(errors, instance, member, attribute, memberValue, rule, fluentResult, resolver, isUnresolvedMessage);
        }
    }

    private static async Task ProcessFluentRuleAsync(FluentRuleAttribute attribute,
    object instance, MemberInfo member, object? memberValue,
    IDictionary<object, object?>? contextItems,
    IValidationRule rule, IValidationMessageResolver resolver, IRuleRegistry registry,
    List<ValidationErrorResult> errors, CancellationToken cancellationToken)
    {
        if (attribute is not IAsyncValidationAttribute asyncAttribute)
            throw new InvalidOperationException($"The specified attribute does not implement {typeof(IAsyncValidationAttribute).Name}.");

        var isUnresolvedMessage = CheckFluentValidationAttribute(attribute, rule, resolver, registry);
        var fluentRuleContext = new ValidationContext(instance, contextItems) { MemberName = member.Name };
        var fluentResult = await asyncAttribute.ValidateAsync(instance, fluentRuleContext, cancellationToken);

        if (fluentResult != ValidationResult.Success)
        {
            AddErrors(errors, instance, member, attribute, memberValue, rule, fluentResult, resolver, isUnresolvedMessage);
        }
    }
}
