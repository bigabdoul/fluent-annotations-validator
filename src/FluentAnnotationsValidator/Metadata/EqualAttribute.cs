using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

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
