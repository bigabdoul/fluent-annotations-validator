using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Extensions;

public static class ValidationRuleBuilderExtensions
{
    public static IValidationRuleBuilder<T, TProp> Compare<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, 
        Expression<Func<T, TProp>> otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal)
    {
        var otherPropertyName = otherProperty.GetMemberInfo().Name;
        return builder.AddRuleFromAttribute(new Compare2Attribute(otherPropertyName, comparison));
    }

    public static IValidationRuleBuilder<T, TProp> Compare<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, 
        string otherProperty, ComparisonOperator comparison = ComparisonOperator.Equal)
        => builder.AddRuleFromAttribute(new Compare2Attribute(otherProperty, comparison));

    public static IValidationRuleBuilder<T, TProp> Empty<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new EmptyAttribute());

    public static IValidationRuleBuilder<T, TProp> NotEmpty<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new NotEmptyAttribute());

    public static IValidationRuleBuilder<T, TProp> ExactLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, int length)
        => builder.Length(length, length);

    public static IValidationRuleBuilder<T, TProp> MinimumLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int minimumLength)
        => builder.Length(minimumLength, -1);

    public static IValidationRuleBuilder<T, TProp> MaximumLength<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int maximumLength)
        => builder.AddRuleFromAttribute(new Length2Attribute(0, maximumLength));

    public static IValidationRuleBuilder<T, TProp> Length<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        int min, int max)
        => builder.AddRuleFromAttribute(new Length2Attribute(min, max));

    public static IValidationRuleBuilder<T, TProp> Required<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new RequiredAttribute());

    public static IValidationRuleBuilder<T, TProp> Equal<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, object? expected)
        => builder.AddRuleFromAttribute(new EqualAttribute<object?>(expected));

    public static IValidationRuleBuilder<T, TProp> Equal<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        Expression<Func<T, TProp>> _, TProp expected, IEqualityComparer<TProp>? equalityComparer = null)
    {
        var attr = new EqualAttribute<TProp>(expected, equalityComparer);
        builder.AddRuleFromAttribute(attr);
        return builder;
    }

    public static IValidationRuleBuilder<T, TProp> NotEqual<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        object? disallowed)
        => builder.AddRuleFromAttribute(new NotEqualAttribute<object?>(disallowed));

    public static IValidationRuleBuilder<T, TProp> NotEqual<T, TProp>(this IValidationRuleBuilder<T, TProp> builder,
        Expression<Func<T, TProp>> _, TProp disallowed, IEqualityComparer<TProp>? equalityComparer = null)
    {
        var attr = new NotEqualAttribute<TProp>(disallowed, equalityComparer);
        builder.AddRuleFromAttribute(attr);
        return builder;
    }

    public static IValidationRuleBuilder<T, TProp> EmailAddress<T, TProp>(this IValidationRuleBuilder<T, TProp> builder)
        => builder.AddRuleFromAttribute(new EmailAddressAttribute());

    internal static ValidationResult GetFailedValidationResult(this ValidationAttribute attribute, object? value, ValidationContext validationContext, 
        IValidationMessageResolver? messageResolver = null)
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