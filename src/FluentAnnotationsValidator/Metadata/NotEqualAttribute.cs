using FluentAnnotationsValidator.Runtime.Validators;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

public class NotEqualAttribute<TProperty>(TProperty unexpectedValue, IEqualityComparer<TProperty>? equalityComparer = null) 
    : FluentValidationAttribute("The field '{0}' must not equal the unexpected value.")
{
    private readonly IEqualityComparer<TProperty> _comparer = equalityComparer ?? EqualityComparer<TProperty>.Default;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is TProperty typedValue)
        {
            if (!_comparer.Equals(typedValue, unexpectedValue))
                return ValidationResult.Success;

            var message = MessageResolver?.ResolveMessage(
                validationContext.ObjectInstance.GetType(),
                validationContext.MemberName ?? validationContext.DisplayName ?? "field",
                this) ?? FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName ?? "field");

            return new ValidationResult($"{message}\nUnexpected: {unexpectedValue}\nActual: {typedValue}");
        }

        return new ValidationResult($"Type mismatch.\nExpected: {typeof(TProperty).Name}\nActual: {value.GetType().Name}");
    }
}