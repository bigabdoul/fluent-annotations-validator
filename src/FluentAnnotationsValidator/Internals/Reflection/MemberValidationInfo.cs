using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Internals.Reflection;

/// <summary>
/// Represents metadata for a single member decorated with 
/// one or more <see cref="ValidationAttribute"/> instances.
/// Used internally to bridge reflection to rule generation.
/// </summary>
public class MemberValidationInfo
{
    /// <summary>
    /// The reflected <see cref="MemberInfo"/> of the model member.
    /// </summary>
    public MemberInfo Member { get; set; } = default!;

    /// <summary>
    /// An array of validation attributes applied to the member.
    /// </summary>
    public ValidationAttribute[] Attributes { get; set; } = [];

    /// <summary>
    /// The target model type to inspect.
    /// </summary>
    public Type DeclaringType { get; set; } = default!;
}
