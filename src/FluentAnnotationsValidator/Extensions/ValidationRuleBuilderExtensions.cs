using FluentAnnotationsValidator.Abstractions;
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
        return builder.AddRuleFromAttribute(new Compare2Attribute(otherPropertyName, comparison));
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
        => builder.AddRuleFromAttribute(new Compare2Attribute(otherProperty, comparison));

    /// <summary>
    /// Adds a rule that ensures the property's value is considered empty.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Empty<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new EmptyAttribute());

    /// <summary>
    /// Adds a rule that ensures the property's value is not empty.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> NotEmpty<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new NotEmptyAttribute());

    /// <summary>
    /// Adds a rule that validates the property has an exact length.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="length">The required exact length.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> ExactLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, int length)
        => builder.AddRuleFromAttribute(new ExactLengthAttribute(length));

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
        => builder.AddRuleFromAttribute(new MinLengthAttribute(minimumLength));

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
        => builder.AddRuleFromAttribute(new MaxLengthAttribute(maximumLength));

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
        => builder.AddRuleFromAttribute(new Length2Attribute(min, max));

    /// <summary>
    /// Specifies that a string property must have a length within a specified range.
    /// </summary>
    /// <remarks>
    /// This extension method applies a <see cref="StringLengthAttribute"/> to the property,
    /// providing a fluent syntax for length validation. This is useful for ensuring that
    /// a string value meets specific length constraints.
    /// </remarks>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated. Must be a string.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="maximumLength">The maximum length of the string.</param>
    /// <param name="minimumLength">The minimum length of the string. Defaults to 0.</param>
    /// <returns>The same builder instance so that multiple rules can be chained.</returns>
    public static IValidationRuleBuilder<T, TProp> StringLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int maximumLength, int minimumLength = 0)
        => builder.AddRuleFromAttribute(new StringLengthAttribute(maximumLength) { MinimumLength = minimumLength });

    /// <summary>
    /// Specifies that a string property must match a regular expression pattern.
    /// </summary>
    /// <remarks>
    /// This extension method applies a <see cref="RegularExpressionAttribute"/> to the property,
    /// providing a fluent syntax for complex pattern matching validation.
    /// </remarks>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated. Must be a string.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <returns>The same builder instance so that multiple rules can be chained.</returns>
    public static IValidationRuleBuilder<T, TProp> RegularExpression<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        string pattern) => builder.AddRuleFromAttribute(new RegularExpressionAttribute(pattern));

    /// <summary>
    /// Specifies that a string property must be a valid credit card number.
    /// </summary>
    /// <remarks>
    /// This extension method applies a <see cref="CreditCardAttribute"/> to the property,
    /// providing a fluent syntax for credit card number validation. The validation
    /// checks for a valid format and checksum.
    /// </remarks>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated. Must be a string.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The same builder instance so that multiple rules can be chained.</returns>
    public static IValidationRuleBuilder<T, TProp> CreditCard<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new CreditCardAttribute());

    /// <summary>
    /// Adds a rule that ensures the property's value is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Required<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new RequiredAttribute());

    /// <summary>
    /// Adds a rule that ensures the property's value is equal to a specified expected value.
    /// </summary>
    /// <typeparam name="T">The type of the object instance being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="expected">The value the property must be equal to.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public static IValidationRuleBuilder<T, TProp> Equal<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, object? expected)
        => builder.AddRuleFromAttribute(new EqualAttribute<object?>(expected));

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
        builder.AddRuleFromAttribute(attr);
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
        => builder.AddRuleFromAttribute(new NotEqualAttribute(disallowed));

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
        builder.AddRuleFromAttribute(attr);
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
        => builder.AddRuleFromAttribute(new EmailAddressAttribute());

    /// <summary>
    /// Specifies that a string property must be a valid phone number.
    /// </summary>
    /// <remarks>
    /// This extension method applies a <see cref="PhoneAttribute"/> to the property,
    /// providing a fluent syntax for validating that a string represents a valid
    /// phone number format.
    /// </remarks>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated. Must be a string.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The same builder instance so that multiple rules can be chained.</returns>
    public static IValidationRuleBuilder<T, TProp> Phone<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new PhoneAttribute());

    /// <summary>
    /// Specifies that a string property must be a valid URL.
    /// </summary>
    /// <remarks>
    /// This extension method applies a <see cref="UrlAttribute"/> to the property,
    /// providing a fluent syntax for validating that a string represents a well-formed
    /// and absolute URL.
    /// </remarks>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated. Must be a string.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <returns>The same builder instance so that multiple rules can be chained.</returns>
    public static IValidationRuleBuilder<T, TProp> Url<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new UrlAttribute());

    /// <summary>
    /// Specifies that a string property must have a file extension that is in a specified list.
    /// </summary>
    /// <remarks>
    /// This extension method applies a <see cref="FileExtensionsAttribute"/> to the property,
    /// providing a fluent syntax for validating file extensions. The attribute checks if the
    /// string value (representing a file name) has an extension that matches one of the
    /// provided extensions.
    /// </remarks>
    /// <typeparam name="T">The type of the model being configured.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated. Must be a string.</typeparam>
    /// <param name="builder">The validation rule builder instance.</param>
    /// <param name="extensions">
    /// An optional comma-separated string of valid file extensions (e.g., "png,jpg,jpeg").
    /// If not specified, the attribute will use its default behavior.
    /// </param>
    /// <returns>The same builder instance so that multiple rules can be chained.</returns>
    public static IValidationRuleBuilder<T, TProp> FileExtensions<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        string? extensions = null)
        => builder.AddRuleFromAttribute(new FileExtensionsAttribute() { Extensions = extensions ?? string.Empty });

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
        int minimum, int maximum) => builder.AddRuleFromAttribute(new RangeAttribute(minimum, maximum));

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
        double minimum, double maximum) => builder.AddRuleFromAttribute(new RangeAttribute(minimum, maximum));

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
        Type type, string minimum, string maximum) => builder.AddRuleFromAttribute(new RangeAttribute(type, minimum, maximum));

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
        => builder.AddRuleFromAttribute(new TAttribute());

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

        var message = messageResolver?.ResolveMessage(
            validationContext.ObjectInstance.GetType(),
            validationContext.MemberName ?? validationContext.DisplayName ?? fieldName,
            attribute) ?? attribute.FormatErrorMessage(fieldName);

        return new ValidationResult(message, [fieldName]);
    }
}