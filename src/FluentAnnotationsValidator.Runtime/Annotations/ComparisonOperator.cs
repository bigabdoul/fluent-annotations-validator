namespace FluentAnnotationsValidator.Runtime.Annotations;

/// <summary>
/// Defines the set of comparison operators that can be used by the <see cref="ComparisonAttribute"/>.
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// Specifies that the values must be equal.
    /// </summary>
    Equal,

    /// <summary>
    /// Specifies that the values must not be equal.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Specifies that the first value must be greater than the second value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Specifies that the first value must be less than the second value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Specifies that the first value must be greater than or equal to the second value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Specifies that the first value must be less than or equal to the second value.
    /// </summary>
    LessThanOrEqual
}