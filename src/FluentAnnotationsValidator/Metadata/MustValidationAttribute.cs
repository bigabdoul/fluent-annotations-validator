using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class MustValidationAttribute<TProperty>(Func<TProperty, bool> predicate) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || predicate((TProperty)value))
            return ValidationResult.Success;

        return new("The specified predicate doesn't satisfy the Must condition.");
    }
}
