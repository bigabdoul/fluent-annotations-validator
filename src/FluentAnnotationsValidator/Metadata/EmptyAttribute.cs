using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that a string, collection, or other countable type must be empty.
/// </summary>
/// <remarks>
/// This attribute validates that a property or field's value represents an empty state.
/// For a <see cref="string"/>, it validates that the value is <see langword="null"/>, an empty string (""), or consists only of white-space characters.
/// For other types, such as collections or arrays, it validates that their count is zero.
/// This attribute can only be applied once to a property or field.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class EmptyAttribute : FluentValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyAttribute"/> class.
    /// </summary>
    public EmptyAttribute() : base("The field '{0}' must be empty.") { }

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) 
            return ValidationResult.Success;

        // For strings, consider null, empty, or whitespace as valid.
        if (value is string s)
        {
            return string.IsNullOrWhiteSpace(s) ? ValidationResult.Success : this.GetFailedValidationResult(validationContext, MessageResolver);
        }

        // For other types, check if a count can be obtained and if it is zero.
        if (CountHelper.TryGetCount(value, out int length))
        {
            return length == 0 ? ValidationResult.Success : this.GetFailedValidationResult(validationContext, MessageResolver);
        }

        // If a count cannot be obtained, this is not a countable type, so we can't validate emptiness.
        // We return success to not fail the validation process unexpectedly.
        return ValidationResult.Success;
    }
}