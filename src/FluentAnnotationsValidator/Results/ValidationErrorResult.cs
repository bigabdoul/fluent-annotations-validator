using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Represents the result of a failed validation.
/// </summary>
public class ValidationErrorResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorResult"/> class.
    /// </summary>
    public ValidationErrorResult()
    {
    }

    /// <summary>
    /// For internal use only. Initializes a new instance of the 
    /// <see cref="ValidationErrorResult"/> class using the specified 
    /// <paramref name="failure"/>.
    /// </summary>
    /// <param name="failure">The validation error that occurred.</param>
    internal ValidationErrorResult(FluentValidationFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        Failure = failure;
    }

    /// <summary>
    /// Gets the internal <see cref="FluentValidationFailure"/> object.
    /// </summary>
    internal FluentValidationFailure? Failure { get; }

    /// <summary>
    /// The property or field being evaluated.
    /// </summary>
    public MemberInfo Member { get; set; } = default!;

    /// <summary>
    /// The name of the property to use over the <see cref="Member"/> object's name.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets the unique key.
    /// </summary>
    public string UniqueKey { get; set; } = default!;

    /// <summary>
    /// Gets the error message for the validation.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The value that was attempted to be validated, which may be null.
    /// </summary>
    public object? AttemptedValue { get; set; }

    /// <summary>
    /// The validation attribute that produced this error, if applicable.
    /// </summary>
    public ValidationAttribute? Attribute { get; set; }

    /// <summary>
    /// Gets or sets the index (for child collections) at which validation failed. Defaults value to -1.
    /// </summary>
    public int ItemIndex { get; set; } = -1;
}