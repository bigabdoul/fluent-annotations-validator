using FluentAnnotationsValidator.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Runtime.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public abstract class FluentValidationAttribute : ValidationAttribute
{
    public FluentValidationAttribute() : base()
    {
    }

    protected FluentValidationAttribute(string errorMessage) : base(errorMessage) { }

    public IValidationMessageResolver? MessageResolver { get; set; }

    public virtual ValidationResult GetFailedValidationResult(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);
        var fieldName = validationContext.DisplayName ?? validationContext.MemberName ?? "field";

        var message = MessageResolver?.ResolveMessage(
            validationContext.ObjectInstance.GetType(),
            validationContext.MemberName ?? string.Empty,
            this) ?? FormatErrorMessage(fieldName);

        return new ValidationResult(message, [fieldName]);
    }
}
