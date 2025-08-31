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

    public object? Expected => expectedValue;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // Use [Required] for null checks

        return _comparer.Equals(value, expectedValue)
                ? ValidationResult.Success
                : this.GetFailedValidationResult(validationContext, MessageResolver);
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class EqualAttribute<TProperty>(TProperty expectedValue, IEqualityComparer<TProperty>? equalityComparer = null) 
    : EqualAttribute(expectedValue, errorMessage: "The field '{0}' must equal the expected value.")
{
    private readonly IEqualityComparer<TProperty> _comparer = equalityComparer ?? EqualityComparer<TProperty>.Default;

    public new TProperty? Expected => (TProperty?)base.Expected;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // Use [Required] for null checks

        return value is TProperty typedValue
            ? _comparer.Equals(typedValue, expectedValue) 
                ? ValidationResult.Success 
                : this.GetFailedValidationResult(validationContext, MessageResolver)
            : new ValidationResult($"Type mismatch.\nExpected: {typeof(TProperty).Name}\nActual: {value.GetType().Name}");
    }
}
