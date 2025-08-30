using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace FluentAnnotationsValidator.Internals.Reflection;

internal static class TypeUtils
{
    internal const BindingFlags StaticPublicNonPublicFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly ConcurrentDictionary<Type, ResourceManager?> _resourceManagerCache = new();
    private static readonly ConcurrentDictionary<(Type, string, string), string?> _localizedStringCache = new();

    /// <summary>
    /// Retrieves the value of a localized resource key exposed as a static property or method 
    /// from a resource class, typically generated from a .resx file.
    /// </summary>
    /// <param name="type">The resource type (e.g. <c>ValidationMessages</c>) containing the key.</param>
    /// <param name="key">The name of the static member to retrieve (e.g. <c>"Email_Required"</c>).</param>
    /// <param name="culture">The UI culture to use.</param>
    /// <returns>
    /// The resolved localized string, or <see langword="null" /> if the key does not exist or retrieval fails.
    /// </returns>
    /// <remarks>
    /// Optimized for performance using two caches: one for resolved localized strings, 
    /// and the other for an internal <see cref="ResourceManager"/> if available.
    /// </remarks>
    public static string? GetResourceValue(this Type type, string key, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var cultureName = culture.Name;

        // Check string-level cache first
        var stringKey = (type, key, cultureName);
        if (_localizedStringCache.TryGetValue(stringKey, out var cachedValue))
            return cachedValue;

        string? value;

        // Method defined in TypeUtils
        if (type.TryGetResourceManager(out var rm))
        {
            value = rm.GetString(key, culture);
        }
        else
        {
            // Fallback to static member (field, property, method)
            var member = type.GetMember(key, StaticPublicNonPublicFlags).FirstOrDefault();

            value = member switch
            {
                PropertyInfo prop => prop.GetValue(null)?.ToString(),
                FieldInfo field => field.GetValue(null)?.ToString(),
                MethodInfo method when method.GetParameters().Length == 0 =>
                    method.Invoke(null, null)?.ToString(),
                _ => null
            };
        }

        _localizedStringCache[stringKey] = value; // Cache resolved value
        return value;
    }

    internal static bool TryGetResourceManager([NotNullWhen(true)] this Type? type, [NotNullWhen(true)] out ResourceManager? rm)
    {
        rm = null;

        if (type == null) return false;

        rm = _resourceManagerCache.GetOrAdd(type, t =>
        {
            var prop = t.GetProperty(nameof(ResourceManager), StaticPublicNonPublicFlags);
            return prop?.GetValue(null) as ResourceManager;
        });

        return rm != null;
    }

    internal static bool TrySetResourceManagerCulture(this Type? type, CultureInfo? culture, bool fallbackToType)
    {
        if (type == null) return false;

        if (type.TryGetResourceManager(out var rm))
            type = rm.GetType();
        else if (!fallbackToType)
            return false;

        var prop = type.GetProperty("Culture", StaticPublicNonPublicFlags);

        if (prop == null) return false;

        prop.SetValue(null, culture ?? Thread.CurrentThread.CurrentCulture);
        return true;
    }

    internal static bool IsAssignableFrom(Type? declaringType, Type? type)
    {
        if (declaringType is null)
            return false;

        // A class is assignable from itself.
        if (declaringType == type)
            return true;

        // Check if the declaring type is a base class of the target type.
        if (type is not null && type.IsSubclassOf(declaringType))
            return true;

        return false;
    }
}
