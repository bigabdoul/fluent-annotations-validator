using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Core.Extensions;

/// <summary>
/// Provides extension methods for custom attributes.
/// </summary>
public static class AttributeExtensions
{
    /// <summary>
    /// Gets a shortened name for a validation attribute by removing the "Attribute" suffix.
    /// </summary>
    /// <param name="attr">The validation attribute instance.</param>
    /// <returns>The shortened attribute name.</returns>
    public static string ShortAttributeName(this ValidationAttribute attr) =>
        attr.CleanAttributeName().Replace("Attribute", string.Empty);

    /// <summary>
    /// Cleans the type name of an attribute to remove common generic type and language-specific suffixes.
    /// </summary>
    /// <param name="attr">The attribute instance.</param>
    /// <returns>The cleaned attribute name.</returns>
    public static string CleanAttributeName(this Attribute attr) =>
        attr.GetType().Name.TrimEnd('`', '1');
}
