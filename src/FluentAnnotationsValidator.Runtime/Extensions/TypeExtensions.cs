using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

namespace FluentAnnotationsValidator.Runtime.Extensions;

using Core.Interfaces;
using FluentAnnotationsValidator.Annotations;

/// <summary>
/// Provides utility methods for working with types, including resource localization and assignability checks.
/// </summary>
public static class TypeExtensions
{
    #region fields

    /// <summary>
    /// Binding flags used to access static public and non-public members.
    /// </summary>
    internal const BindingFlags StaticPublicNonPublicFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Caches <see cref="ResourceManager"/> instances for resource types to avoid repeated reflection.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ResourceManager?> _resourceManagerCache = new();

    /// <summary>
    /// Caches resolved localized strings by type, key, and culture name.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type, string, string), string?> _localizedStringCache = new();
    
    #endregion

    /// <summary>
    /// Parses and converts <see cref="ValidationAttribute"/> instances into runtime validation rules.
    /// </summary>
    /// <remarks>
    /// Handles multiple instances of the same attribute type, preserving uniqueness.
    /// </remarks>
    /// <param name="instanceType">The target model type.</param>
    /// <param name="member">The property or field to inspect belonging to <paramref name="instanceType"/>.</param>
    /// <returns>A list of conditional validation rules for the member.</returns>
    public static List<IValidationRule> ParseRules(this Type instanceType, MemberInfo member)
    {
        // Retrieve all validation attributes directly applied to the member (property, field, etc.)
        ValidationAttribute[] attributes = [.. member.GetCustomAttributes<ValidationAttribute>(inherit: true)];

        // Include class-level attributes that opt into rule inheritance (async and sync variants)
        var classAsyncAttributes = instanceType.GetCustomAttributes<InheritRulesAsyncAttribute>(inherit: true);
        var classAttributes = instanceType.GetCustomAttributes<InheritRulesAttribute>(inherit: true);

        // Merge class-level async attributes into the member-level attribute list
        if (classAsyncAttributes.Any())
        {
            attributes = [.. attributes.Union(classAsyncAttributes)];
        }

        // Merge class-level sync attributes into the member-level attribute list
        if (classAttributes.Any())
        {
            attributes = [.. attributes.Union(classAttributes)];
        }

        // Create a default validation expression that targets the member itself.
        // This is used when no fluent override is present.
        LambdaExpression defaultExpression = (object instance) => member;

        // Initialize the rule list to collect all applicable validation rules
        var rules = new List<IValidationRule>();

        // If no attributes are found, but the type supports fluent validation,
        // register a placeholder rule to allow fluent overrides later.
        if (attributes.Length == 0)
        {
            if (typeof(IFluentValidatable).IsAssignableFrom(instanceType))
                AddRule($"{instanceType.Namespace}.{instanceType.Name}.{member.Name}", null);
        }
        else
        {
            // For each attribute, generate a unique key and register a validation rule
            foreach (var attr in attributes)
            {
                var uniqueKey = $"[{attr.GetType().Name}:{attr.TypeId}]{instanceType.Namespace}.{instanceType.Name}.{member.Name}";
                AddRule(uniqueKey, attr);
            }
        }

        // Return the collected validation rules for the member
        return rules;

        // Registers a validation rule for the current member.
        void AddRule(string uniqueKey, ValidationAttribute? attr)
        {
            // Always validate unless overridden by fluent rules.
            var rule = new ValidationRule(defaultExpression)
            {
                Member = member,
                Validator = attr,
                UniqueKey = uniqueKey,
            };
            rules.Add(rule);
        }
    }

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

    /// <summary>
    /// Attempts to retrieve a <see cref="ResourceManager"/> instance from the specified resource type.
    /// </summary>
    /// <param name="type">The resource type to inspect.</param>
    /// <param name="rm">When this method returns, contains the <see cref="ResourceManager"/> if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if a <see cref="ResourceManager"/> was successfully retrieved; otherwise, <c>false</c>.</returns>
    public static bool TryGetResourceManager([NotNullWhen(true)] this Type? type, [NotNullWhen(true)] out ResourceManager? rm)
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

    /// <summary>
    /// Attempts to set the culture of a resource manager associated with the specified type.
    /// </summary>
    /// <param name="type">The resource type or resource manager type.</param>
    /// <param name="culture">The culture to apply.</param>
    /// <param name="fallbackToType">
    /// If <c>true</c>, fallback to using the type itself when no <see cref="ResourceManager"/> is found.
    /// </param>
    /// <returns><c>true</c> if the culture was successfully set; otherwise, <c>false</c>.</returns>
    public static bool TrySetResourceManagerCulture(this Type? type, CultureInfo? culture, bool fallbackToType)
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
}
