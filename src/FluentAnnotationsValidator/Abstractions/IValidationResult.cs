using FluentAnnotationsValidator.Results;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Represents a contract for a validation result that contains a collection of errors.
/// </summary>
public interface IValidationResult
{
    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    List<FluentValidationFailure> Errors { get; }
}
