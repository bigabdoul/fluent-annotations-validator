using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Represents the result of a failed validation.
/// </summary>
public class ValidationErrorResult
{
    /// <summary>
    /// The property or field being evaluated.
    /// </summary>
    public MemberInfo Member { get; init; } = default!;

    /// <summary>
    /// Gets the unique key.
    /// </summary>
    public string UniqueKey { get; init; } = default!;

    /// <summary>
    /// Gets the error message for the validation.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// The value that was attempted to be validated, which may be null.
    /// </summary>
    public object? AttemptedValue { get; init; }

    /// <summary>
    /// The validation attribute that produced this error, if applicable.
    /// </summary>
    public ValidationAttribute? Attribute { get; init; }
}
