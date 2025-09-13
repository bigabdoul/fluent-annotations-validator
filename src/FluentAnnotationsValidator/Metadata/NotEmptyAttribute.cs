using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that a string, collection, or other countable type must not be empty.
/// </summary>
/// <remarks>
/// This attribute validates that a property or field's value is not empty.
/// For a <see cref="string"/>, it validates that the value is not <see langword="null"/>,
/// not an empty string (""), and does not consist only of white-space characters.
/// For other types, such as collections or arrays, it validates that their count is greater than zero.
/// This attribute can only be applied once to a property or field.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotEmptyAttribute : FluentValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotEmptyAttribute"/> class.
    /// </summary>
    public NotEmptyAttribute() : base("The field '{0}' must not be empty.") { }

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Explicit null check first
        if (value is null)
            return this.GetFailedValidationResult(validationContext, MessageResolver);

        // For strings, consider null, empty, or whitespace as invalid.
        if (value is string s)
        {
            return string.IsNullOrWhiteSpace(s) ? this.GetFailedValidationResult(validationContext, MessageResolver) : ValidationResult.Success;
        }

        // For other types, check if a count can be obtained and if it is greater than zero.
        if (CountHelper.TryGetCount(value, out int length))
        {
            return length > 0
                ? ValidationResult.Success
                : this.GetFailedValidationResult(validationContext, MessageResolver);
        }

        // If a count cannot be obtained, this is not a countable type, so we can't validate emptiness.
        // We return success to not fail the validation process unexpectedly.
        return ValidationResult.Success;
    }
}