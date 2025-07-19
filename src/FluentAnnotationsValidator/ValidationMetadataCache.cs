using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

/// <summary>
/// Caches property-level <see cref="ValidationAttribute"/> metadata for any type.
/// Reduces repeated reflection cost and accelerates validator generation.
/// </summary>
public static class ValidationMetadataCache
{
    private static readonly ConcurrentDictionary<Type, PropertyValidationInfo[]> _cache = new();

    /// <summary>
    /// Retrieves a cached array of <see cref="PropertyValidationInfo"/> for the provided type.
    /// If no cache entry exists, it reflects the type and stores results for future reuse.
    /// </summary>
    /// <param name="type">The target model type to inspect.</param>
    /// <returns>An array of property metadata, each linked to relevant <see cref="ValidationAttribute"/>s.</returns>
    public static PropertyValidationInfo[] Get(Type type)

    {
        return _cache.GetOrAdd(type, t =>
            [.. t.GetProperties()
             .Where(p => p.CanRead)
             .Select(p => new PropertyValidationInfo
             {
                 Property = p,
                 Attributes = [.. p.GetCustomAttributes<ValidationAttribute>(true)]
             })
             .Where(p => p.Attributes.Length != 0)]
        );
    }
}
