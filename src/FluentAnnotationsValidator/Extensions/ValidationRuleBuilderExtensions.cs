using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Extensions;

/// <summary>
/// Provides a set of fluent extension methods for <see cref="IValidationRuleBuilder{T, TProp}"/>
/// to define common validation rules using a more expressive syntax.
/// </summary>
public static class ValidationRuleBuilderExtensions
{
    /// <summary>
    /// Defines a validation rule for child properties within the current rule's property.
    /// </summary>
    /// <typeparam name="T">The type of the root object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated, which contains the children.</typeparam>
    /// <param name="builder">The current rule builder instance.</param>
    /// <param name="configure">An action that contains the configuration for the child validators.</param>
    /// <returns>The same rule builder instance for a fluid chain.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the configure action is null.</exception>
    public static IFluentTypeValidator<TProp> ChildRules<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
    Action<FluentTypeValidator<TProp>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var current = builder.CurrentRule as IValidationRule<T>;
        ArgumentNullException.ThrowIfNull(current?.Expression);

        var typeValidator = new FluentTypeValidator<TProp>(new(builder.Registry));

        configure(typeValidator);

        CollectionValidatorBase<TProp> attr = builder.IsAsync
            ? new CollectionValidatorAsyncAttribute<TProp>
            {
                RuleRegistry = builder.Registry,
            }
            : new CollectionValidatorAttribute<TProp>
            {
                RuleRegistry = builder.Registry,
            };

        var rules = typeValidator.Build();
        attr.Rules.AddRange(rules.OfType<IValidationRule<TProp>>());

        var parentMember = current.Expression.GetMemberInfo();
        var childRule = current.CreateValidationRule<TProp>(parentMember);
        childRule.Validator = attr;

        builder.AddChildRule(childRule);
        typeValidator.DiscardRulesFromLastBuild();

        return typeValidator;
    }

    /// <summary>
    /// Adds a rule to compare the current property's value with another property's value
    /// on the same object instance.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="otherProperty">An expression identifying the property to compare against.</param>
    /// <param name="comparison">The type of comparison to perform, defaulting to <see cref="ComparisonOperator.Equal"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Compare<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        Expression<Func<T, TProp>> otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal)
    {
        var otherPropertyName = otherProperty.GetMemberInfo().Name;
        return builder.SetValidator(new Compare2Attribute(otherPropertyName, comparison));
    }

    /// <summary>
    /// Adds a rule to compare the current property's value with another property's value
    /// on the same object instance, specified by a string name.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="otherProperty">The name of the property to compare against.</param>
    /// <param name="comparison">The type of comparison to perform, defaulting to <see cref="ComparisonOperator.Equal"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Compare<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        string otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal)
        => builder.SetValidator(new Compare2Attribute(otherProperty, comparison));

    /// <summary>
    /// Adds a rule that ensures the property's value is considered empty.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Empty<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.SetValidator(new EmptyAttribute());

    /// <summary>
    /// Adds a rule that ensures the property's value is not empty.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> NotEmpty<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.SetValidator(new NotEmptyAttribute());

    /// <summary>
    /// Adds a rule that validates the property has an exact length.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="length">The required exact length.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> ExactLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, int length)
        => builder.SetValidator(new ExactLengthAttribute(length));

    /// <summary>
    /// Adds a rule that validates the property has a minimum length.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="minimumLength">The minimum allowed length.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> MinimumLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int minimumLength)
        => builder.SetValidator(new MinLengthAttribute(minimumLength));

    /// <summary>
    /// Adds a rule that validates the property has a maximum length.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="maximumLength">The maximum allowed length.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> MaximumLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int maximumLength)
        => builder.SetValidator(new MaxLengthAttribute(maximumLength));

    /// <summary>
    /// Adds a rule that validates the property's length falls within a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="min">The minimum allowed length.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Length<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int min, int max)
        => builder.SetValidator(new Length2Attribute(min, max));

    /// <summary>
    /// Adds a rule that ensures the property's value is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Required<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.SetValidator(new RequiredAttribute());

    /// <summary>
    /// Adds a rule that ensures the property's value is equal to a specified expected value.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="expected">The value the property must be equal to.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Equal<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, object? expected)
        => builder.SetValidator(new EqualAttribute<object?>(expected));

    /// <summary>
    /// Adds a rule that ensures the property's value is equal to a specified expected value,
    /// using an optional custom equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="_">An unused expression to help with type inference.</param>
    /// <param name="expected">The value the property must be equal to.</param>
    /// <param name="equalityComparer">An optional comparer to use for the equality check.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Equal<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        Expression<Func<T, TProp>> _, TProp expected, IEqualityComparer<TProp>? equalityComparer = null)
    {
        var attr = new EqualAttribute<TProp>(expected, equalityComparer);
        builder.SetValidator(attr);
        return builder;
    }

    /// <summary>
    /// Adds a rule that ensures the property's value is not equal to a specified disallowed value.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="disallowed">The value the property must not be equal to.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> NotEqual<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        object? disallowed)
        => builder.SetValidator(new NotEqualAttribute(disallowed));

    /// <summary>
    /// Adds a rule that ensures the property's value is not equal to a specified disallowed value,
    /// using an optional custom equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="_">An unused expression to help with type inference.</param>
    /// <param name="disallowed">The value the property must not be equal to.</param>
    /// <param name="equalityComparer">An optional comparer to use for the inequality check.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> NotEqual<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        Expression<Func<T, TProp>> _, TProp disallowed, IEqualityComparer<TProp>? equalityComparer = null)
    {
        var attr = new NotEqualAttribute<TProp>(disallowed, equalityComparer);
        builder.SetValidator(attr);
        return builder;
    }

    /// <summary>
    /// Adds a rule that validates a property's value as a valid email address.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> EmailAddress<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.SetValidator(new EmailAddressAttribute());

    /// <summary>
    /// Specifies that a property's value must be within a specified range of integer values.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The current builder instance.</param>
    /// <param name="minimum">The minimum allowable integer value.</param>
    /// <param name="maximum">The maximum allowable integer value.</param>
    /// <returns>A reference to the current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Range<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int minimum, int maximum) => builder.SetValidator(new RangeAttribute(minimum, maximum));

    /// <summary>
    /// Specifies that a property's value must be within a specified range of double values.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The current builder instance.</param>
    /// <param name="minimum">The minimum allowable double value.</param>
    /// <param name="maximum">The maximum allowable double value.</param>
    /// <returns>A reference to the current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Range<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        double minimum, double maximum) => builder.SetValidator(new RangeAttribute(minimum, maximum));

    /// <summary>
    /// Specifies that a property's value must be within a specified range of values of a given type.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The current builder instance.</param>
    /// <param name="type">The type of the object to be compared.</param>
    /// <param name="minimum">The string representation of the minimum value.</param>
    /// <param name="maximum">The string representation of the maximum value.</param>
    /// <returns>A reference to the current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Range<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        Type type, string minimum, string maximum) => builder.SetValidator(new RangeAttribute(type, minimum, maximum));
    #region MinimumAttribute

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, byte value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, short value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, int value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, long value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, Int128 value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, IntPtr value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, float value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, double value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, decimal value)
        => builder.SetValidator(new MinimumAttribute(value));

    #region unsigned overloads

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, ushort value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, uint value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, ulong value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, UIntPtr value)
        => builder.SetValidator(new MinimumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be greater than or equal to the given minimum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The minimum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Minimum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, UInt128 value)
        => builder.SetValidator(new MinimumAttribute(value));

    #endregion

    #endregion

    #region MaximumAttribute

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, byte value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, short value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, int value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, long value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, Int128 value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, IntPtr value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, float value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, double value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, decimal value)
        => builder.SetValidator(new MaximumAttribute(value));

    #region unsigned overloads

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, ushort value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, uint value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, ulong value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, UIntPtr value)
        => builder.SetValidator(new MaximumAttribute(value));

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, UInt128 value)
        => builder.SetValidator(new MaximumAttribute(value));

    #endregion

    #endregion

    /// <summary>
    /// Adds a rule by instantiating and attaching a specified <see cref="ValidationAttribute"/> to the rule builder.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <typeparam name="TAttribute">The type of the <see cref="ValidationAttribute"/> to attach.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> WithAttribute<T, TProp, TAttribute>(this IValidationRuleBuilder<T, TProp> builder)
        where TAttribute : ValidationAttribute, new()
        => builder.SetValidator(new TAttribute());

    /// <summary>
    /// Gets a failed <see cref="ValidationResult"/> instance with a resolved error message,
    /// falling back to the attribute's default message if a resolver is not provided.
    /// </summary>
    /// <param name="attribute">The <see cref="ValidationAttribute"/> that failed.</param>
    /// <param name="validationContext">The context of the validation operation.</param>
    /// <param name="messageResolver">
    /// An optional message resolver to use for retrieving a localized or custom error message.
    /// </param>
    /// <returns>A new <see cref="ValidationResult"/> representing the validation failure.</returns>
    internal static ValidationResult GetFailedValidationResult(this ValidationAttribute attribute, ValidationContext validationContext, IValidationMessageResolver? messageResolver = null)
    {
        ArgumentNullException.ThrowIfNull(validationContext);
        var fieldName = validationContext.DisplayName ?? validationContext.MemberName ?? "field";

        var message = messageResolver?.ResolveMessage
        (
            validationContext.ObjectInstance,
            validationContext.MemberName ?? validationContext.DisplayName ?? fieldName,
            attribute,
            (attribute as FluentValidationAttribute)?.Rule
        ) ?? attribute.FormatErrorMessage(fieldName);

        return new ValidationResult(message, [fieldName]);
    }
}
