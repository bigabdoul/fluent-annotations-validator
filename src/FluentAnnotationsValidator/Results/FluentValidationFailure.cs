using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Represents a validation failure in FluentValidation.
/// </summary>
/// <remarks>
/// This class is a wrapper around the FluentValidation's `ValidationFailure` class.
/// </remarks>
[Serializable]
public class FluentValidationFailure
{
    /// <summary>
	/// Creates a new fluent validation failure.
	/// </summary>
	public FluentValidationFailure()
    {
    }

    /// <summary>
    /// Creates a new fluent validation failure.
    /// </summary>
    public FluentValidationFailure(string propertyName, string errorMessage) : this(propertyName, errorMessage, null)
    {
    }

    /// <summary>
    /// Creates a new FluentValidationFailure.
    /// </summary>
    public FluentValidationFailure(string propertyName, string errorMessage, object? attemptedValue)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }

    /// <summary>
    /// Copy constructor: Creates a new FluentValidationFailure from an existing ValidationFailure.
    /// </summary>
    /// <param name="failure">The existing <see cref="ValidationFailure"/> to copy properties from.</param>
    protected internal FluentValidationFailure(ValidationFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        PropertyName = failure.PropertyName;
        ErrorMessage = failure.ErrorMessage;
        AttemptedValue = failure.AttemptedValue;
        CustomState = failure.CustomState;
        ErrorCode = failure.ErrorCode;
        FormattedMessagePlaceholderValues = failure.FormattedMessagePlaceholderValues;
    }

    /// <summary>
    /// The name of the property.
    /// </summary>
    public virtual string PropertyName { get; set; } = default!;

    /// <summary>
    /// The error message
    /// </summary>
    public virtual string ErrorMessage { get; set; } = default!;

    /// <summary>
    /// The property value that caused the failure.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public virtual object? AttemptedValue { get; set; }

    /// <summary>
    /// Custom state associated with the failure.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual object? CustomState { get; set; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the formatted message placeholder values.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual Dictionary<string, object>? FormattedMessagePlaceholderValues { get; set; }

    /// <summary>
    /// Creates a textual representation of the failure.
    /// </summary>
    public override string ToString()
    {
        return ErrorMessage;
    }
}