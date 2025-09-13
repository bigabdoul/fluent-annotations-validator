using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that a set of reusable validation rules should be applied to a member.
/// The rules are sourced from a class that has been configured with the validator.
/// </summary>
/// <remarks>
/// This attribute enables the reuse of complex validation logic by referencing a type
/// that contains the configured rules.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="FluentRuleAttribute"/> class.
/// </remarks>
/// <param name="objectType">The type that holds the validation rules.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public sealed class FluentRuleAttribute(Type objectType) : ValidationAttribute
{
    /// <summary>
    /// Gets the type that contains the configured validation rules to be applied.
    /// </summary>
    public Type ObjectType { get; } = objectType ?? throw new ArgumentNullException(nameof(objectType));
}
