using FluentAnnotationsValidator.Internals.Annotations;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace FluentAnnotationsValidator.Runtime.Helpers;

internal static class CountHelper
{
    private static readonly ConcurrentDictionary<Type, Func<object, int>?> _countExtractors = [];

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

    private static Func<object, int>? CreateExtractor(Type type)
    {
        if (typeof(string).IsAssignableFrom(type))
            return obj => ((string)obj).Length;

        if (typeof(Array).IsAssignableFrom(type))
            return obj => ((Array)obj).Length;

        if (typeof(ICollection).IsAssignableFrom(type))
            return obj => ((ICollection)obj).Count;

        // generic ICollection<T>
        var iCollectionGeneric = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));

        if (iCollectionGeneric != null)
        {
            var countProp = iCollectionGeneric.GetProperty("Count");
            if (countProp != null)
                return obj => (int)countProp.GetValue(obj)!;
        }

        // IReadOnlyCollection<T>
        var iReadOnlyCollectionGeneric = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));

        if (iReadOnlyCollectionGeneric != null)
        {
            var countProp = iReadOnlyCollectionGeneric.GetProperty("Count");
            if (countProp != null)
                return obj => (int)countProp.GetValue(obj)!;
        }

        // Custom [CountProperty] support
        var prop = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p =>
                p.GetCustomAttribute<CountPropertyAttribute>() != null &&
                p.PropertyType == typeof(int) &&
                p.CanRead);

        if (prop != null)
        {
            return obj => (int)prop.GetValue(obj)!;
        }

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