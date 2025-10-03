using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace "FluentAnnotationsValidator" does not match folder structure, expected "FluentAnnotationsValidator.Runtime.Extensions"

namespace FluentAnnotationsValidator;

#pragma warning restore IDE0130

using Annotations;
using Core.Extensions;
using Runtime;
using Runtime.Annotations;

/// <summary>
/// Provides fluent chaining extension methods for instances
/// of the <see cref="FluentTypeValidator{T}"/> class.
/// </summary>
public static class FluentTypeValidatorExtensions
{
    /// <summary>
    /// Adds a comparison rule to the current configuration, comparing the last-defined member
    /// with another property on the same model, specified by an expression.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="otherProperty">An expression identifying the property to compare against.</param>
    /// <param name="comparison">The comparison operator to use, defaulting to <see cref="ComparisonOperator.Equal"/>.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Compare<T, TProp>(this FluentTypeValidator<T> configurator,
        Expression<Func<T, TProp>> otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal)
    {
        var otherPropertyName = otherProperty.GetMemberInfo().Name;
        return configurator.AddValidator(new ComparisonAttribute(otherPropertyName, comparison));
    }

    /// <summary>
    /// Adds a comparison rule to the current configuration, comparing the last-defined member
    /// with another property on the same model, specified by its string name.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="otherProperty">The string name of the property to compare against.</param>
    /// <param name="comparison">The comparison operator to use, defaulting to <see cref="ComparisonOperator.Equal"/>.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Compare<T>(this FluentTypeValidator<T> configurator,
        string otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal, Predicate<T>? when = null)
        => configurator.AddValidator(new ComparisonAttribute(otherProperty, comparison), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member is considered empty.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Empty<T>(this FluentTypeValidator<T> configurator,
        Predicate<T>? when = null)
        => configurator.AddValidator(new EmptyAttribute(), when);

    /// <summary>
    /// Attaches a <see cref="NotEmptyAttribute"/> to the most recent fluent rule definition.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> NotEmpty<T>(this FluentTypeValidator<T> configurator,
        Predicate<T>? when = null)
        => configurator.AddValidator(new NotEmptyAttribute(), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member has an exact length.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="length">The required exact length.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> ExactLength<T>(this FluentTypeValidator<T> configurator,
        int length, Predicate<T>? when = null)
        => configurator.AddValidator(new ExactLengthAttribute(length), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member has a minimum length.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="minimumLength">The minimum allowed length.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> MinimumLength<T>(this FluentTypeValidator<T> configurator,
        int minimumLength, Predicate<T>? when = null)
        => configurator.AddValidator(new MinLengthAttribute(minimumLength), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member has a maximum length.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="maximumLength">The maximum allowed length.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> MaximumLength<T>(this FluentTypeValidator<T> configurator,
        int maximumLength, Predicate<T>? when = null)
        => configurator.AddValidator(new MaxLengthAttribute(maximumLength), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member's length is within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="min">The minimum allowed length.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Length<T>(this FluentTypeValidator<T> configurator,
        int min, int max, Predicate<T>? when = null)
        => configurator.AddValidator(new LengthCountAttribute(min, max), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Required<T>(this FluentTypeValidator<T> configurator,
        Predicate<T>? when = null)
        => configurator.AddValidator(new RequiredAttribute(), when);

    /// <summary>
    /// Adds a rule that ensures the last-defined member is equal to a specified expected value.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="expected">The value the member must be equal to.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Equal<T>(this FluentTypeValidator<T> configurator,
        object? expected, Predicate<T>? when = null)
        => configurator.AddValidator(new EqualAttribute<object?>(expected), when);

    /// <summary>
    /// Adds a rule that ensures a member's value is equal to a specified expected value,
    /// with optional custom equality comparison.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property to compare.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="_">An unused expression to help with type inference.</param>
    /// <param name="expected">The value the member must be equal to.</param>
    /// <param name="equalityComparer">An optional comparer to use for the equality check.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> Equal<T, TProp>(this FluentTypeValidator<T> configurator,
        Expression<Func<T, TProp>> _, TProp expected, IEqualityComparer<TProp>? equalityComparer = null,
        Predicate<T>? when = null)
    {
        var attr = new EqualAttribute<TProp>(expected, equalityComparer);
        configurator.AddValidator(attr, when);
        return configurator;
    }

    /// <summary>
    /// Adds a rule that ensures the last-defined member is not equal to a specified disallowed value.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="disallowed">The value the member must not be equal to.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> NotEqual<T>(this FluentTypeValidator<T> configurator,
        object? disallowed, Predicate<T>? when = null)
        => configurator.AddValidator(new NotEqualAttribute<object?>(disallowed), when);

    /// <summary>
    /// Adds a rule that ensures a member's value is not equal to a specified disallowed value,
    /// with optional custom equality comparison.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property to compare.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="_">An unused expression to help with type inference.</param>
    /// <param name="disallowed">The value the member must not be equal to.</param>
    /// <param name="equalityComparer">An optional comparer to use for the inequality check.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> NotEqual<T, TProp>(this FluentTypeValidator<T> configurator,
        Expression<Func<T, TProp>> _, TProp disallowed, IEqualityComparer<TProp>? equalityComparer = null,
        Predicate<T>? when = null)
    {
        var attr = new NotEqualAttribute<TProp>(disallowed, equalityComparer);
        configurator.AddValidator(attr, when);
        return configurator;
    }

    /// <summary>
    /// Adds a rule that validates the last-defined member's value as a valid email address.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> EmailAddress<T>(this FluentTypeValidator<T> configurator,
        Predicate<T>? when = null)
        => configurator.AddValidator(new EmailAddressAttribute(), when);

    /// <summary>
    /// Attaches a custom validation attribute instance to the last-defined member in the fluent chain.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TAttribute">The type of the <see cref="ValidationAttribute"/> to attach.</typeparam>
    /// <param name="configurator">The configurator for the specified <typeparamref name="T"/>.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <returns>A reference to the <paramref name="configurator"/> instance for method chaining.</returns>
    public static FluentTypeValidator<T> WithAttribute<T, TAttribute>(this FluentTypeValidator<T> configurator,
        Predicate<T>? when = null) where TAttribute : ValidationAttribute, new()
        => configurator.AddValidator(new TAttribute(), when);

    /// <summary>
    /// Generates a conventional localization key for a validation attribute and a member name.
    /// </summary>
    /// <param name="_">Unused parameter: The declaring type.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <returns>The generated conventional key.</returns>
    /// <param name="attr">The validation attribute instance.</param>
    public static string GetConventionalKey(this Type _, string memberName, ValidationAttribute attr)
        => $"{memberName}_{attr.ShortAttributeName()}";

    /// <summary>
    /// Gets a shortened name for a validation attribute by removing the "Attribute" suffix.
    /// </summary>
    /// <param name="attr">The validation attribute instance.</param>
    /// <returns>The shortened attribute name.</returns>
    public static string ShortAttributeName(this ValidationAttribute attr) =>
        attr.CleanAttributeName().Replace("Attribute", string.Empty);

    /// <summary>
    /// Cleans the type name of an attribute to remove common generic type and language-specific suffixes.
    /// </summary>
    /// <param name="attr">The attribute instance.</param>
    /// <returns>The cleaned attribute name.</returns>
    public static string CleanAttributeName(this Attribute attr) =>
        attr.GetType().Name.TrimEnd('`', '1');
    /// <summary>
    /// Creates a new <see cref="ValidationRule{T}"/> from a pending rule instance.
    /// </summary>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <param name="rule">The pending rule to convert.</param>
    /// <param name="member">The member to which the rule applies.</param>
    /// <param name="attribute">The validation attribute to associate with the rule.</param>
    /// <param name="when">An optional predicate that determines whether this rule should be applied.</param>
    /// <param name="asyncCondition">A function that evaluates when a rule is applied asynchronously.</param>
    /// <returns>A new <see cref="ValidationRule{T}"/> instance.</returns>
    internal static ValidationRule<T> CreateRuleFromPending<T>(this PendingRule<T> rule,
    MemberInfo member, ValidationAttribute? attribute = null,
    Predicate<T>? when = null, Func<T, CancellationToken, Task<bool>>? asyncCondition = null)
    {
        if (when != null)
        {
            rule.Condition = when;
        }

        var validationRule = new ValidationRule<T>(
            rule.Condition,
            rule.Expression,
            rule.Message,
            rule.Key,
            rule.ResourceKey,
            rule.ResourceType,
            rule.Culture,
            rule.FallbackMessage,
            rule.UseConventionalKeys ?? true)
        {
            Member = member,
            Validator = attribute,
            ConfigureBeforeValidation = rule.ConfigureBeforeValidation,
        };

        if (asyncCondition != null)
        {
            validationRule.AsyncCondition = asyncCondition;
        }

        return validationRule;
    }
}