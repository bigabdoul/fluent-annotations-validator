using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace FluentAnnotationsValidator.Annotations;

/// <summary>
/// Provides utility methods for extracting count information from various object types.
/// </summary>
public static class CountHelper
{
    /// <summary>
    /// A cache of type-specific count extractor delegates for performance optimization.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Func<object, int>?> _countExtractors = [];

    /// <summary>
    /// Attempts to extract a count value from the specified object.
    /// </summary>
    /// <param name="value">The object to inspect for count information.</param>
    /// <param name="count">When this method returns, contains the extracted count if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if a count could be extracted; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Supports strings, arrays, <see cref="ICollection"/>, <see cref="ICollection{T}"/>, <see cref="IReadOnlyCollection{T}"/>,
    /// custom types with a property marked by <see cref="CountPropertyAttribute"/>, and any <see cref="IEnumerable"/>.
    /// </remarks>
    public static bool TryGetCount(object? value, out int count)
    {
        if (value is null)
        {
            count = 0;
            return false;
        }

        var type = value.GetType();

        var extractor = _countExtractors.GetOrAdd(type, CreateExtractor);
        if (extractor is null)
        {
            count = 0;
            return false;
        }

        count = extractor(value);
        return true;
    }

    /// <summary>
    /// Creates a delegate that can extract a count from an object of the specified type.
    /// </summary>
    /// <param name="type">The type to analyze for count extraction logic.</param>
    /// <returns>
    /// A delegate that returns an integer count from an object of the given type,
    /// or <c>null</c> if the type is not supported.
    /// </returns>
    private static Func<object, int>? CreateExtractor(Type type)
    {
        if (typeof(string).IsAssignableFrom(type))
            return obj => ((string)obj).Length;

        if (typeof(Array).IsAssignableFrom(type))
            return obj => ((Array)obj).Length;

        if (typeof(ICollection).IsAssignableFrom(type))
            return obj => ((ICollection)obj).Count;

        // Support for generic ICollection<T>
        var iCollectionGeneric = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));

        if (iCollectionGeneric?.GetProperty("Count") is PropertyInfo countProp)
            return obj => (int)countProp.GetValue(obj)!;

        // Support for IReadOnlyCollection<T>
        var iReadOnlyCollectionGeneric = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));

        if (iReadOnlyCollectionGeneric?.GetProperty("Count") is PropertyInfo readOnlyCountProp)
            return obj => (int)readOnlyCountProp.GetValue(obj)!;

        // Support for custom [CountProperty] attribute
        var prop = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p =>
                p.GetCustomAttribute<CountPropertyAttribute>() != null &&
                p.PropertyType == typeof(int) &&
                p.CanRead);

        if (prop != null)
            return obj => (int)prop.GetValue(obj)!;

        // Fallback for IEnumerable
        if (typeof(IEnumerable).IsAssignableFrom(type))
            return obj =>
            {
                int count = 0;
                foreach (var _ in (IEnumerable)obj)
                    count++;
                return count;
            };

        return null; // Unsupported type
    }
}