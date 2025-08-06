using FluentAnnotationsValidator.Runtime.Validators;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Internals.Annotations;

public class EqualAttribute<TProperty>(TProperty expectedValue, IEqualityComparer<TProperty>? equalityComparer = null) 
    : FluentValidationAttribute("The field '{0}' must equal the expected value.")
{
    private readonly IEqualityComparer<TProperty> _comparer = equalityComparer ?? EqualityComparer<TProperty>.Default;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success; // Use [Required] for null checks

        return value is TProperty typedValue
            ? _comparer.Equals(typedValue, expectedValue) 
                ? ValidationResult.Success 
                : GetFailedValidationResult(value, validationContext)
            : new ValidationResult($"Type mismatch.\nExpected: {typeof(TProperty).Name}\nActual: {value.GetType().Name}");
    }
}
