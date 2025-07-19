using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

public static class ValidationMetadataCache
{
    private static readonly ConcurrentDictionary<Type, PropertyValidationInfo[]> _cache = new();

    public static PropertyValidationInfo[] Get(Type type)
    {
        return _cache.GetOrAdd(type, t =>
            t.GetProperties()
             .Where(p => p.CanRead)
             .Select(p => new PropertyValidationInfo
             {
                 Property = p,
                 Attributes = p.GetCustomAttributes<ValidationAttribute>(true).ToArray()
             })
             .Where(p => p.Attributes.Any())
             .ToArray()
        );
    }
}
