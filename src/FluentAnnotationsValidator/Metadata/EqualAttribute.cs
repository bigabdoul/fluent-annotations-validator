using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that a property or field's value must be equal to a specified expected value.
/// </summary>
/// <remarks>
/// This attribute validates that the value of the decorated property is equal to a provided
/// expected value using the specified <see cref="IEqualityComparer{T}"/>, or the default
/// comparer if one is not provided. It is a more flexible alternative to <see cref="CompareAttribute"/>,
/// which is designed for comparing two properties on the same object.
/// <para>
/// This attribute does not validate against <see langword="null"/> values. Use the
/// <see cref="RequiredAttribute"/> for that purpose.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class EqualAttribute(object? expectedValue, IEqualityComparer<object?>? equalityComparer = null,
    string errorMessage = "The field '{0}' must equal the expected value.") : FluentValidationAttribute(errorMessage)
{
    private readonly IEqualityComparer<object> _comparer = equalityComparer ?? EqualityComparer<object?>.Default;

    /// <summary>
    /// Gets the expected value that the property or field must equal.
    /// </summary>
    public object? Expected => expectedValue;

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // Use [Required] for null checks

        return _comparer.Equals(value, expectedValue)
                ? ValidationResult.Success
                : this.GetFailedValidationResult(validationContext, MessageResolver);
    }
}

/// <summary>
/// A type-safe version of the <see cref="EqualAttribute"/> for a specific property type.
/// </summary>
/// <remarks>
/// This generic attribute provides compile-time type checking for the expected value,
/// ensuring that it matches the type of the property being validated. It also performs
/// a runtime check to ensure the validated value is of the expected type before
/// performing the equality comparison.
/// </remarks>
/// <typeparam name="TProperty">The type of the property to validate.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class EqualAttribute<TProperty>(TProperty expectedValue, IEqualityComparer<TProperty>? equalityComparer = null)
    : EqualAttribute(expectedValue, errorMessage: "The field '{0}' must equal the expected value.")
{
    private readonly IEqualityComparer<TProperty> _comparer = equalityComparer ?? EqualityComparer<TProperty>.Default;

    /// <summary>
    /// Gets the type-safe expected value that the property or field must equal.
    /// </summary>
    public new TProperty? Expected => (TProperty?)base.Expected;

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // Use [Required] for null checks

        if (value is TProperty typedValue)
        {
            return _comparer.Equals(typedValue, expectedValue)
                ? ValidationResult.Success
                : this.GetFailedValidationResult(validationContext, MessageResolver);
        }
        else
        {
            return new ValidationResult($"Type mismatch.\nExpected: {typeof(TProperty).Name}\nActual: {value.GetType().Name}");
        }
    }
}