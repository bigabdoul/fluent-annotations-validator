using FluentAnnotationsValidator.Runtime.Helpers;
using FluentAnnotationsValidator.Runtime.Validators;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotEmptyAttribute : FluentValidationAttribute
{
    public NotEmptyAttribute() : base("The field '{0}' must not be empty.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string s && string.IsNullOrWhiteSpace(s))
            return GetFailedValidationResult(value, validationContext);

        if (!CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success;

        return length > 0 ? ValidationResult.Success : GetFailedValidationResult(value, validationContext);
    }
}

