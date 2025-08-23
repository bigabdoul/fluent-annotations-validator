namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Represents the result of a validation operation using FluentAnnotationsValidator.
/// </summary>
[Serializable]
public class FluentValidationResult
{
    /// <summary>
    /// Represents a successful validation result with no errors.
    /// </summary>
    public static FluentValidationResult Success => new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationResult"/> class.
    /// </summary>
    public FluentValidationResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationResult"/> class.
    /// </summary>
    /// <param name="errors">A list of validation errors encountered during the validation process.</param>
    public FluentValidationResult(List<FluentValidationFailure> errors) => Errors = errors;

    /// <summary>
    /// A list of validation errors encountered during the validation process.
    /// </summary>
    public List<FluentValidationFailure> Errors { get; } = [];

    /// <summary>
    /// Indicates whether the validation result is valid (i.e., contains no errors).
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}
