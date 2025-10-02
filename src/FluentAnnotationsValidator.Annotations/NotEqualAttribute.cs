using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Annotations;

/// <summary>
/// Specifies that a property or field's value must not be equal to a specified unexpected value.
/// </summary>
/// <remarks>
/// This attribute validates that the value of the decorated property is not equal to a provided
/// unexpected value using the specified <see cref="IEqualityComparer{T}"/>, or the default
/// comparer if one is not provided.
/// <para>
/// This attribute does not validate against <see langword="null"/> values.
/// Use the <see cref="RequiredAttribute"/> for that purpose.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NotEqualAttribute(object? unexpectedValue, IEqualityComparer<object?>? equalityComparer = null,
    string errorMessage = "The field '{0}' must not equal the unexpected value.")
    : FluentValidationAttribute(errorMessage)
{
    private readonly IEqualityComparer<object?> _comparer = equalityComparer ?? EqualityComparer<object?>.Default;

    /// <summary>
    /// Gets the unexpected value that the property or field must not equal.
    /// </summary>
    public object? Unexpected => unexpectedValue;

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (!_comparer.Equals(value, unexpectedValue))
            return ValidationResult.Success;

        var message = MessageResolver?.ResolveMessage
        (
            validationContext.ObjectInstance
,
            validationContext.MemberName ?? validationContext.DisplayName ?? "field",
            this) ?? FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName ?? "field");

        return new ValidationResult($"{message}\nUnexpected: {unexpectedValue}\nActual: {value}");
    }
}

/// <summary>
/// A type-safe version of the <see cref="NotEqualAttribute"/> for a specific property type.
/// </summary>
/// <remarks>
/// This generic attribute provides compile-time type checking for the unexpected value,
/// ensuring that it matches the type of the property being validated. It also performs
/// a runtime check to ensure the validated value is of the expected type before
/// performing the inequality comparison.
/// </remarks>
/// <typeparam name="TProperty">The type of the property to validate.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NotEqualAttribute<TProperty>(TProperty unexpectedValue, IEqualityComparer<TProperty>? equalityComparer = null)
    : NotEqualAttribute(unexpectedValue)
{
    private readonly IEqualityComparer<TProperty> _comparer = equalityComparer ?? EqualityComparer<TProperty>.Default;

    /// <summary>
    /// Gets the type-safe unexpected value that the property or field must not equal.
    /// </summary>
    public new TProperty? Unexpected => (TProperty?)base.Unexpected;

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is TProperty typedValue)
        {
            if (!_comparer.Equals(typedValue, unexpectedValue))
                return ValidationResult.Success;

            var message = MessageResolver?.ResolveMessage
            (
                validationContext.ObjectInstance
,
                validationContext.MemberName ?? validationContext.DisplayName ?? "field",
                this) ?? FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName ?? "field");

            return new ValidationResult($"{message}\nUnexpected: {unexpectedValue}\nActual: {typedValue}");
        }

        return new ValidationResult($"Type mismatch.\nExpected: {typeof(TProperty).Name}\nActual: {value.GetType().Name}");
    }
}