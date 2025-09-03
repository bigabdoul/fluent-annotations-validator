using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Provides a set of static methods for aggregating validation errors from multiple rules
/// into a unified result set.
/// </summary>
public static class ValidationResultAggregator
{
    /// <summary>
    /// Synchronously validates all rules for a given member and returns error results.
    /// This method evaluates conditional rules and executes attribute-based validations
    /// on the member's value.
    /// </summary>
    /// <param name="rules">The conditional rules to evaluate.</param>
    /// <param name="type">The declaring type of the member.</param>
    /// <param name="instance">The model instance containing the member.</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the property or field.</param>
    /// <param name="resolver">An object used for validation message resolution.</param>
    /// <returns>A list of <see cref="ValidationErrorResult"/> if any rules failed; an empty list otherwise.</returns>
    public static List<ValidationErrorResult> Validate(
        this IEnumerable<ConditionalValidationRule> rules,
        Type type,
        object instance,
        MemberInfo member,
        IValidationMessageResolver resolver)
    {
        if (!rules.Any())
        {
            return [];
        }

        // Check if there are any fluent rules and if their conditions are met.
        var hasFluentRule = rules.Any(r => !r.HasAttribute);
        if (hasFluentRule)
        {
            var anyFluentConditionApplies = rules
                .Where(r => !r.HasAttribute)
                .Any(r => r.ShouldApply(instance));

            if (!anyFluentConditionApplies)
            {
                // If there are fluent rules but none of their conditions are met, skip validation for this member.
                return [];
            }
        }

        var errors = new List<ValidationErrorResult>();
        var value = member.GetValue(instance);
        var context = new ValidationContext(instance) { MemberName = member.Name };
        var preconfigurationInvoked = false;

        foreach (var rule in rules)
        {
            if (!rule.ShouldApply(instance))
            {
                continue;
            }

            if (!preconfigurationInvoked)
            {
                GetPrevalidationValue(rules, instance, member, ref value, ref preconfigurationInvoked);
            }

            if (rule.Attribute is { } attr)
            {
                var result = attr.GetValidationResult(value, context);
                if (result != ValidationResult.Success)
                {
                    var message = resolver.ResolveMessage(type, member.Name, attr, rule);
                    errors.Add(new ValidationErrorResult
                    {
                        AttemptedValue = value,
                        Member = member,
                        Message = message ?? result?.ErrorMessage ?? "Validation failed.",
                        UniqueKey = rule.UniqueKey,
                        Attribute = attr,
                    });
                }
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
    /// <param name="type">The declaring type of the member.</param>
    /// <param name="instance">The model instance containing the member.</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the property or field.</param>
    /// <param name="resolver">An object used for validation message resolution.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a list of <see cref="ValidationErrorResult"/> if any rules failed;
    /// an empty list otherwise.
    /// </returns>
    public static async Task<List<ValidationErrorResult>> ValidateAsync(
        this IEnumerable<ConditionalValidationRule> rules,
        Type type,
        object instance,
        MemberInfo member,
        IValidationMessageResolver resolver,
        CancellationToken cancellationToken = default)
    {
        if (!rules.Any())
        {
            return [];
        }

        var hasFluentRule = rules.Any(r => !r.HasAttribute);
        if (hasFluentRule)
        {
            var fluentConditionTasks = rules
                .Where(r => !r.HasAttribute)
                .Select(r => r.ShouldApplyAsync(instance, cancellationToken));

            var fluentConditionsMet = await Task.WhenAll(fluentConditionTasks);

            if (!fluentConditionsMet.Any(x => x))
            {
                return [];
            }
        }

        var errors = new List<ValidationErrorResult>();
        var value = member.GetValue(instance);
        var context = new ValidationContext(instance) { MemberName = member.Name };
        var preconfigurationInvoked = false;

        foreach (var rule in rules)
        {
            if (!await rule.ShouldApplyAsync(instance, cancellationToken))
            {
                continue;
            }

            if (!preconfigurationInvoked)
            {
                GetPrevalidationValue(rules, instance, member, ref value, ref preconfigurationInvoked);
            }

            if (rule.Attribute is { } attr)
            {
                var result = attr.GetValidationResult(value, context);
                if (result != ValidationResult.Success)
                {
                    var message = resolver.ResolveMessage(type, member.Name, attr, rule);
                    errors.Add(new ValidationErrorResult
                    {
                        AttemptedValue = value,
                        Member = member,
                        Message = message ?? result?.ErrorMessage ?? "Validation failed.",
                        UniqueKey = rule.UniqueKey,
                        Attribute = attr,
                    });
                }
            }
        }

        return errors;
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
    private static void GetPrevalidationValue(IEnumerable<ConditionalValidationRule> rules,
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
}
