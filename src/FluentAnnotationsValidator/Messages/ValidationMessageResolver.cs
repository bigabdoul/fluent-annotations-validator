using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

namespace FluentAnnotationsValidator.Messages;

/// <summary>
/// Provides mechanisms for resolving localized error messages associated with <see cref="ValidationAttribute"/> instances.
/// Supports conventional lookup, explicit resource naming, and fallback formatting strategies.
/// </summary>
public class ValidationMessageResolver(ValidationBehaviorOptions options) : IValidationMessageResolver
{
    private const BindingFlags StaticPublicNonPublicFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private static readonly ConcurrentDictionary<Type, ResourceManager?> _resourceManagerCache = new();
    private static readonly ConcurrentDictionary<(Type, string, string), string?> _localizedStringCache = new();

    /// <inheritdoc cref="IValidationMessageResolver.ResolveMessage{T}(Expression{Func{T, string?}}, ValidationAttribute, ConditionalValidationRule?)"/>
    public string? ResolveMessage<T>(Expression<Func<T, string?>> expression, ValidationAttribute attr, ConditionalValidationRule? rule = null)
        => ResolveMessage(typeof(T), expression.GetMemberInfo().Name, attr, rule);

    /// <summary>
    /// Resolves the error message to be used for a validation failure, based on the supplied
    /// <see cref="ValidationAttribute"/>, property metadata, and optional conditional rule context.
    /// </summary>
    /// <param name="declaringType">
    /// The metadata container for the property, or field being validated, 
    /// including its <see cref="MemberInfo"/> and target model type.
    /// </param>
    /// <param name="attr">
    /// The <see cref="ValidationAttribute"/> instance describing the validation logic and message configuration.
    /// </param>
    /// <param name="rule">
    /// Optional: The <see cref="ConditionalValidationRule"/> representing conditional validation logic.
    /// May contain metadata overrides such as <c>ResourceKey</c> or <c>Message</c>.
    /// </param>
    /// <returns>
    /// A fully formatted error message string to display to consumers (e.g., UI or diagnostics).
    /// Returns <see langword="null" /> if no message can be resolved.
    /// </returns>
    public virtual string? ResolveMessage(Type declaringType, string memberName, ValidationAttribute attr, ConditionalValidationRule? rule = null)
    {
        // 1️ Rule-based explicit message override
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.Message))
        {
            return rule.Message;
        }

        var formatArg = GetFormatValue(attr);
        var culture = rule?.Culture ?? options.CommonCulture ?? CultureInfo.CurrentUICulture;
        var resourceType = rule?.ResourceType ?? options.CommonResourceType;

        // 2️ Rule-based resource lookup
        if (rule is not null &&
            !string.IsNullOrWhiteSpace(rule.ResourceKey) &&
            resourceType is not null &&
            TryResolveFromResource(resourceType, rule.ResourceKey, culture, formatArg, out var resolvedFromRule))
        {
            return resolvedFromRule;
        }

        // 3️ Attribute-based explicit resource key
        if (!string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var explicitType = attr.ErrorMessageResourceType
                ?? declaringType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

            if (explicitType is not null &&
                TryResolveFromResource(explicitType, attr.ErrorMessageResourceName!, culture, formatArg, out var resolvedFromAttr))
            {
                return resolvedFromAttr;
            }
        }

        // 4️ Convention fallback via [ValidationResource]
        var fallbackType = declaringType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

        if (fallbackType is not null && (rule?.UseConventionalKeyFallback ?? options.UseConventionalKeyFallback))
        {
            var key = rule?.ResourceKey ?? GetConventionalKey(memberName, attr);

            if (TryResolveFromResource(fallbackType, key, culture, formatArg, out var resolvedFromConvention))
                return resolvedFromConvention;
        }

        // 5️ Rule-level fallback message
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.FallbackMessage))
            return rule.FallbackMessage;

        // 6️ Inline message or final fallback
        return !string.IsNullOrWhiteSpace(attr.ErrorMessage)
            ? attr.FormatErrorMessage(memberName)
            : $"Invalid value for {memberName}";
    }

    /// <summary>
    /// Attempts to resolve a localized error message from a specified resource type and key.
    /// </summary>
    /// <param name="resourceType">The resource class containing static message properties.</param>
    /// <param name="resourceKey">The property name to retrieve.</param>
    /// <param name="culture">
    /// Optional: A specific <see cref="CultureInfo"/> to format the resolved message.
    /// If <see langword="null" />, defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </param>
    /// <param name="formatArg">Optional: An argument used to format the resolved message string.</param>
    /// <param name="message">
    /// When this method returns, contains the resolved and formatted message if found;
    /// otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if resolution succeeded and a non-null message was returned;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool TryResolveFromResource(Type resourceType, string resourceKey, CultureInfo? culture,
        object? formatArg, [NotNullWhen(true)] out string? message)
    {
        message = null;

        if (resourceType is null || string.IsNullOrWhiteSpace(resourceKey))
            return false;

        culture ??= CultureInfo.CurrentUICulture;

        // try to retrieve a localized message from string resources
        var raw = GetResourceValue(resourceType, resourceKey, culture);

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        try
        {
            message = FormatMessage(culture, raw, formatArg);

            return true;
        }
        catch (FormatException)
        {
            // Gracefully fail and null the message if invalid format args
            message = null;
            return false;
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
    public static string? GetResourceValue(Type type, string key, CultureInfo? culture = null)
    {
        var cultureName = (culture ?? CultureInfo.CurrentUICulture).Name;

        // Check string-level cache first
        var stringKey = (type, key, cultureName);
        if (_localizedStringCache.TryGetValue(stringKey, out var cachedValue))
            return cachedValue;

        var rm = _resourceManagerCache.GetOrAdd(type, t =>
        {
            var prop = t.GetProperty("ResourceManager", StaticPublicNonPublicFlags);
            return prop?.GetValue(null) as ResourceManager;
        });

        string? value = null;

        if (rm != null)
        {
            value = rm.GetString(key, culture ?? CultureInfo.CurrentCulture);
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
    /// Extracts contextual formatting data from common <see cref="ValidationAttribute"/> types, 
    /// which can be injected into error messages as format arguments (e.g. min/max lengths).
    /// </summary>
    /// <param name="attr">The validation attribute instance to inspect.</param>
    /// <returns>
    /// An object representing the format argument(s), such as an integer, array, or pattern string. 
    /// Returns <see langword="null" /> if the attribute type is unsupported or lacks relevant data.
    /// </returns>
    protected static object? GetFormatValue(ValidationAttribute attr)
    {
        return attr switch
        {
            MinLengthAttribute m => m.Length,
            MaxLengthAttribute m => m.Length,
            StringLengthAttribute s => s.MinimumLength > 0
                ? new[] { s.MinimumLength, s.MaximumLength }
                : s.MaximumLength,
            RangeAttribute r => new[] { r.Minimum, r.Maximum },
            RegularExpressionAttribute r => r.Pattern,
            CompareAttribute c => c.OtherProperty, // for "must match {0}" style messages
            CreditCardAttribute => "credit-card",   // semantic format type
            EmailAddressAttribute => "email",         // useful for diagnostics or fallback tagging
            PhoneAttribute => "phone",         // helps with custom formatting or error hints
            UrlAttribute => "url",           // gives context to string-based formats
            FileExtensionsAttribute f => f.Extensions,    // could return string.Join(", ", f.Extensions)
            RequiredAttribute => "required",      // placeholder-friendly (e.g. "{0} is required")
            _ => null
        };
    }

    /// <summary>
    /// Formats a message string using the specified culture and format argument(s), with support 
    /// for both scalar values and array-based inputs.
    /// </summary>
    /// <param name="culture">The culture used for formatting conventions.</param>
    /// <param name="format">The composite format string (e.g. <c>"Value must be between {0} and {1}."</c>).</param>
    /// <param name="args">
    /// The value or values to inject into the format string. Can be a single object, 
    /// an <c>object[]</c>, or a typed array like <c>int[]</c>.
    /// </param>
    /// <returns>The fully formatted and culture-aware message string.</returns>
    protected static string FormatMessage(CultureInfo culture, string format, object? args)
    {
        // Dynamically unpack array-based formatArg for string.Format — supports object[], int[], etc.
        // Falls back to single value formatting if not array
        return args switch
        {
            object[] list => string.Format(culture, format, list),
            Array arr => string.Format(culture, format, [.. arr.Cast<object>()]),
            _ => string.Format(culture, format, args)
        };
    }

    internal static string GetConventionalKey(string memberName, ValidationAttribute attr)
    {
        var shortName = attr.GetType().Name.Replace("Attribute", "");
        return $"{memberName}_{shortName}";
    }
}
