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
}