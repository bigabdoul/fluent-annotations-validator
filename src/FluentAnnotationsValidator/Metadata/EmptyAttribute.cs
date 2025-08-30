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
        if (validationContext.ObjectType == typeof(string) && string.IsNullOrWhiteSpace((string?)value) || !CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success;

        return length == 0 ? ValidationResult.Success : this.GetFailedValidationResult(validationContext, MessageResolver);
    }
}
