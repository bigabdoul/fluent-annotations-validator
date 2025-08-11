using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotEmptyAttribute : FluentValidationAttribute
{
    public NotEmptyAttribute() : base("The field '{0}' must not be empty.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string s && string.IsNullOrWhiteSpace(s))
            return this.GetFailedValidationResult(value, validationContext, MessageResolver);

        if (!CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success;

        return length > 0 
            ? ValidationResult.Success 
            : this.GetFailedValidationResult(value, validationContext, MessageResolver);
    }
}

