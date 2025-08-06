using FluentAnnotationsValidator.Runtime.Validators;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Internals.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotEmptyAttribute : FluentValidationAttribute
{
    public NotEmptyAttribute() : base("The field '{0}' must not be empty.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Null is valid — use [Required] for presence
        if (value == null)
            return ValidationResult.Success;

        bool isEmpty =
            value is string s && string.IsNullOrWhiteSpace(s) ||
            value is ICollection c && c.Count == 0 ||
            value is IEnumerable e && !e.GetEnumerator().MoveNext();

        return isEmpty ? GetFailedValidationResult(value, validationContext) : ValidationResult.Success;
    }
}

