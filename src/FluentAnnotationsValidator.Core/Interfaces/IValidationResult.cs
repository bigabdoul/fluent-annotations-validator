using FluentAnnotationsValidator.Core.Results;

namespace FluentAnnotationsValidator.Core.Interfaces;

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
