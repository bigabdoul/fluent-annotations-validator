using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Aggregates validation errors from multiple rules into a unified result set.
/// </summary>
public static class ValidationResultAggregator
{
    /// <summary>
    /// Validates all rules for a given member and returns error results.
    /// </summary>
    /// <param name="rules">The conditional rules to evaluate.</param>
    /// <param name="type">The declaring type.</param>
    /// <param name="instance">An instance of the declaring <paramref name="type"/>.</param>
    /// <param name="member">The property or field of the declaring <paramref name="type"/>.</param>
    /// <param name="resolver">An object used for validation message resolution.</param>
    /// <returns>A list of errors if any rules failed; empty otherwise.</returns>
    public static List<ValidationErrorResult> Validate(
        this IEnumerable<ConditionalValidationRule> rules,
        Type type, object instance, MemberInfo member,
        IValidationMessageResolver resolver)
    {

        // Determine if fluent rule applies for this member (i.e., any condition returns true)
        bool fluentConditionApplies = rules
            .Where(r => !r.HasAttribute)
            .Any(r => r.ShouldApply(instance));

        // If any fluent rule applies, proceed with full evaluation
        // If no fluent rule exists OR all conditions failed → skip validation
        if (!fluentConditionApplies && rules.All(r => r.HasAttribute))
        {
            // No conditional gate, treat as fully passive attribute-backed rules
            // Let attribute rules run as default behavior
        }
        else if (!fluentConditionApplies)
        {
            // Fluent condition is the activation gate — skip member entirely
            return [];
        }

        var errors = new List<ValidationErrorResult>();
        var value = GetValue(member, instance);
        var context = new ValidationContext(instance) { MemberName = member.Name };

        foreach (var rule in rules)
        {
            if (!rule.ShouldApply(instance)) continue;

            if (rule.Attribute is { } attr)
            {
                var result = attr.GetValidationResult(value, context);
                if (result != ValidationResult.Success)
                {
                    var message = resolver.ResolveMessage(type, member.Name, attr, rule);
                    errors.Add(new ValidationErrorResult
                    {
                        Member = member,
                        UniqueKey = rule.UniqueKey,
                        Message = message ?? result?.ErrorMessage ?? "Validation failed."
                    });
                }
            }

            // Future support: custom fluent-only rules could be evaluated here
        }

        return errors;
    }

    internal static object? GetValue(MemberInfo member, object instance)
    {
        return member switch
        {
            PropertyInfo prop => prop.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            MethodInfo method when method.GetParameters().Length == 0 =>
                method.Invoke(instance, null),
            _ => null
        };
    }
}
