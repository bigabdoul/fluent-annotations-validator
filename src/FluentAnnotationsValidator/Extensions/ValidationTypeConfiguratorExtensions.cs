using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Extensions;

public static class ValidationTypeConfiguratorExtensions
{
    public static ValidationTypeConfigurator<T> Compare<T, TProp>(this ValidationTypeConfigurator<T> configurator,
        Expression<Func<T, TProp>> otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal)
    {
        var otherPropertyName = otherProperty.GetMemberInfo().Name;
        return configurator.AttachAttribute(new Compare2Attribute(otherPropertyName, comparison));
    }

    public static ValidationTypeConfigurator<T> Compare<T>(this ValidationTypeConfigurator<T> configurator,
        string otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal, Func<T, bool>? when = null)
        => configurator.AttachAttribute(new Compare2Attribute(otherProperty, comparison), when);

    public static ValidationTypeConfigurator<T> Empty<T>(this ValidationTypeConfigurator<T> configurator,
        Func<T, bool>? when = null)
        => configurator.AttachAttribute(new EmptyAttribute(), when);

    /// <summary>
    /// Attach rule to most recent .Rule(...) call; use for fluent chaining.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configurator"></param>
    /// <returns></returns>
    public static ValidationTypeConfigurator<T> NotEmpty<T>(this ValidationTypeConfigurator<T> configurator,
        Func<T, bool>? when = null)
        => configurator.AttachAttribute(new NotEmptyAttribute(), when);

    public static ValidationTypeConfigurator<T> ExactLength<T>(this ValidationTypeConfigurator<T> configurator, 
        int length, Func<T, bool>? when = null)
        => configurator.Length(length, length, when);

    public static ValidationTypeConfigurator<T> MinimumLength<T>(this ValidationTypeConfigurator<T> configurator, 
        int minimumLength, Func<T, bool>? when = null)
        => configurator.Length(minimumLength, -1, when);

    public static ValidationTypeConfigurator<T> MaximumLength<T>(this ValidationTypeConfigurator<T> configurator, 
        int maximumLength, Func<T, bool>? when = null)
        => configurator.AttachAttribute(new Length2Attribute(0, maximumLength), when);

    public static ValidationTypeConfigurator<T> Length<T>(this ValidationTypeConfigurator<T> configurator, 
        int min, int max, Func<T, bool>? when = null)
        => configurator.AttachAttribute(new Length2Attribute(min, max), when);

    public static ValidationTypeConfigurator<T> Required<T>(this ValidationTypeConfigurator<T> configurator, 
        Func<T, bool>? when = null)
        => configurator.AttachAttribute(new RequiredAttribute(), when);

    public static ValidationTypeConfigurator<T> Equal<T>(this ValidationTypeConfigurator<T> configurator, 
        object? expected, Func<T, bool>? when = null)
        => configurator.AttachAttribute(new EqualAttribute<object?>(expected), when);

    public static ValidationTypeConfigurator<T> Equal<T, TProp>(this ValidationTypeConfigurator<T> configurator,
        Expression<Func<T, TProp>> _, TProp expected, IEqualityComparer<TProp>? equalityComparer = null, 
        Func<T, bool>? when = null)
    {
        var attr = new EqualAttribute<TProp>(expected, equalityComparer);
        configurator.AttachAttribute(attr, when);
        return configurator;
    }

    public static ValidationTypeConfigurator<T> NotEqual<T>(this ValidationTypeConfigurator<T> configurator, 
        object? disallowed, Func<T, bool>? when = null)
        => configurator.AttachAttribute(new NotEqualAttribute<object?>(disallowed), when);

    public static ValidationTypeConfigurator<T> NotEqual<T, TProp>(this ValidationTypeConfigurator<T> configurator,
        Expression<Func<T, TProp>> _, TProp disallowed, IEqualityComparer<TProp>? equalityComparer = null,
        Func<T, bool>? when = null)
    {
        var attr = new NotEqualAttribute<TProp>(disallowed, equalityComparer);
        configurator.AttachAttribute(attr, when);
        return configurator;
    }

    public static ValidationTypeConfigurator<T> EmailAddress<T>(this ValidationTypeConfigurator<T> configurator,
        Func<T, bool>? when = null)
        => configurator.AttachAttribute(new EmailAddressAttribute(), when);

    public static ValidationTypeConfigurator<T> WithAttribute<T, TAttribute>(this ValidationTypeConfigurator<T> configurator,
        Func<T, bool>? when = null) where TAttribute : ValidationAttribute, new()
        => configurator.AttachAttribute(new TAttribute(), when);

    internal static ConditionalValidationRule CreateRuleFromPending<T>(this PendingRule<T> rule, MemberInfo member, ValidationAttribute? attribute = null, Func<T, bool>? when = null)
    {
        if (when is not null)
        {
            rule.Predicate = model => when(model);
        }

        var conditionalRule = new ConditionalValidationRule(
            model => rule.Predicate((T)model),
            rule.Message,
            rule.Key,
            rule.ResourceKey,
            rule.ResourceType,
            rule.Culture,
            rule.FallbackMessage,
            rule.UseConventionalKeys ?? true)
        {
            Member = member,
            Attribute = attribute,
        };

        return conditionalRule;
    }
}
