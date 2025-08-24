using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// Converts <see cref="ValidationAttribute"/> instances into runtime validation rules.
/// Handles multiple instances of the same attribute type, preserving uniqueness.
/// </summary>
public static class ValidationAttributeAdapter
{
    /// <summary>
    /// Parses all validation attributes from a property,
    /// creating one or more <see cref="ConditionalValidationRule"/> entries.
    /// </summary>
    /// <param name="instanceType">The target model type.</param>
    /// <param name="member">The property or field to inspect belonging to <paramref name="instanceType"/>.</param>
    /// <returns>A list of conditional validation rules for the member.</returns>
    public static List<ConditionalValidationRule> ParseRules(Type instanceType, MemberInfo member)
    {
        ValidationAttribute[] attributes = [..member.GetCustomAttributes<ValidationAttribute>(inherit: true)];

        if (attributes.Length == 0) return [];

        var rules = new List<ConditionalValidationRule>();

        foreach (var attr in attributes)
        {
            var uniqueKey = $"[{attr.GetType().Name}:{attr.TypeId}]{instanceType.Namespace}.{instanceType.Name}.{member.Name}";
            var rule = new ConditionalValidationRule(Predicate: _ => true) // always validate, unless fluent overrides occur
            {
                Member = member,
                Attribute = attr,
                UniqueKey = uniqueKey,
                ResourceKey = ValidationMessageResolver.GetConventionalKey(member.Name, attr),
            };
            rules.Add(rule);
        }

        return rules;
    }
}
