using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

/// <summary>
/// Represents metadata for a single property decorated with one or more <see cref="ValidationAttribute"/> instances.
/// Used internally to bridge reflection to rule generation.
/// </summary>
public class PropertyValidationInfo
{
    /// <summary>
    /// The reflected <see cref="PropertyInfo"/> of the model property.
    /// </summary>
    public PropertyInfo Property { get; set; } = default!;

    /// <summary>
    /// An array of validation attributes applied to the property.
    /// </summary>
    public ValidationAttribute[] Attributes { get; set; } = [];
}
