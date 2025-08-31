using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace FluentAnnotationsValidator.Results;

/// <summary>
/// Represents a validation failure in FluentAnnotationsValidator.
/// </summary>
[Serializable]
public class FluentValidationFailure
{
    private readonly ValidationErrorResult? _error;

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
    /// Creates a new instance of the <see cref="FluentValidationFailure"/> class.
    /// </summary>
    public FluentValidationFailure(string propertyName, string errorMessage, object? attemptedValue)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationFailure"/> class using the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The validation error result.</param>
    public FluentValidationFailure(ValidationErrorResult error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _error = error;
        PropertyName = error.Member.Name;
        ErrorMessage = error.Message ?? string.Empty;
        AttemptedValue = error.AttemptedValue;
        CustomState = error.Attribute is null ? null : $"Origin: {error.Attribute.CleanAttributeName()}";
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
    /// Gets the validation error result, if any, that was used to 
    /// initialize this <see cref="FluentValidationFailure"/> instance.
    /// </summary>
    /// <returns>A reference to an instance of the <see cref="ValidationErrorResult"/> class, or <see langword="null"/>.</returns>
    public ValidationErrorResult? GetValidationErrorResult() => _error;

    /// <summary>
    /// Creates a textual representation of the failure.
    /// </summary>
    public override string ToString() => ErrorMessage;

    /// <summary>
    /// Determines whether the other type is equal to the current <see cref="FluentValidationFailure"/>.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(FluentValidationFailure? other)
    {
        if (other is null || _error is null || other._error is null)
            return false;

        return IsSameRule((_error.Member, _error.Attribute), (other._error.Member, other._error.Attribute));
    }

    public override bool Equals(object? obj) => Equals(obj as FluentValidationFailure);

    public override int GetHashCode() => HashCode.Combine(_error?.Member.Name, _error?.Attribute?.GetType());

    static bool IsSameRule((MemberInfo, ValidationAttribute?) source, (MemberInfo, ValidationAttribute?) target)
    {
        var (member1, attr1) = source;
        var (member2, attr2) = target;
        return member1.AreSameMembers(member2) && attr1?.GetType() == attr2?.GetType();
    }
}