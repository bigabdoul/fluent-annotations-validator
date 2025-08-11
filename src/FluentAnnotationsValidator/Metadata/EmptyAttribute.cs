using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class EmptyAttribute : FluentValidationAttribute
{
    public EmptyAttribute() : base("The field '{0}' must be empty.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string s && string.IsNullOrWhiteSpace(s) || !CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success;

        return length == 0 ? ValidationResult.Success : this.GetFailedValidationResult(value, validationContext, MessageResolver);
    }
}
