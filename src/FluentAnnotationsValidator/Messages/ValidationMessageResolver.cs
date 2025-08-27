using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Metadata;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace FluentAnnotationsValidator.Messages;

/// <summary>
/// Provides mechanisms for resolving localized error messages associated with <see cref="ValidationAttribute"/> instances.
/// Supports conventional lookup, explicit resource naming, and fallback formatting strategies.
/// </summary>
public class ValidationMessageResolver(ValidationBehaviorOptions options, IStringLocalizerFactory localizerFactory) : IValidationMessageResolver
{
    /*
    /// <summary>
    /// Resolves the error message to be used for a validation failure, based on the supplied
    /// <see cref="ValidationAttribute"/>, property metadata, and optional conditional rule context.
    /// </summary>
    /// <param name="declaringType">
    /// The metadata container for the property, field, or parameter being validated, including its <see cref="PropertyInfo"/> and target model type.
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
    /// Returns <c>null</c> if no message can be resolved.
    /// </returns>
    public virtual string? ResolveMessage(Type declaringType, string memberName, ValidationAttribute attr, ConditionalValidationRule? rule = null)
    {
        // 1️ Rule-based explicit message override
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.Message))
        {
            return rule.Message;
        }

        var formatArg = GetFormatValue(attr);
        var culture = rule?.Culture ?? options.CommonCulture ?? CultureInfo.CurrentCulture;
        var resourceType = rule?.ResourceType ?? options.CommonResourceType;

        // 2️ Rule-based resource lookup
        if (rule is not null &&
            !string.IsNullOrWhiteSpace(rule.ResourceKey) &&
            resourceType is not null &&
            TryResolveFromResource(resourceType, rule.ResourceKey!, culture, formatArg, out var resolvedFromRule))
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

        if (fallbackType is not null && (rule?.UseConventionalKeyFallback ?? options.UseConventionalKeys))
        {
            var key = rule?.ResourceKey ?? GetConventionalKey(memberName, attr);

            if (TryResolveFromResource(fallbackType, key, culture, formatArg, out var resolvedFromConvention))
                return resolvedFromConvention;
        }

        // 5️ Rule-level fallback message
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.FallbackMessage))
            return rule.FallbackMessage;

        // 6️ Inline message or final fallback
        string? message = null;

        try { message = attr.FormatErrorMessage(memberName); }
        catch {}

        return !string.IsNullOrWhiteSpace(message)
            ? message
            : $"Invalid value for {memberName}";
    }
    */

    /// <summary>
    /// Resolves the error message to be used for a validation failure, based on the supplied
    /// <see cref="ValidationAttribute"/>, property metadata, and optional conditional rule context.
    /// </summary>
    /// <param name="declaringType">
    /// The metadata container for the property, field, or parameter being validated, including its <see cref="PropertyInfo"/> and target model type.
    /// </param>
    /// <param name="memberName">The name of the member being validated.</param>
    /// <param name="attr">
    /// The <see cref="ValidationAttribute"/> instance describing the validation logic and message configuration.
    /// </param>
    /// <param name="rule">
    /// Optional: The <see cref="ConditionalValidationRule"/> representing conditional validation logic.
    /// May contain metadata overrides such as <c>ResourceKey</c> or <c>Message</c>.
    /// </param>
    /// <returns>
    /// A fully formatted error message string to display to consumers (e.g., UI or diagnostics).
    /// Returns <c>null</c> if no message can be resolved.
    /// </returns>
    public virtual string? ResolveMessage(Type declaringType, string memberName, ValidationAttribute attr, ConditionalValidationRule? rule = null)
    {
        // 1️ Rule-based explicit message override
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.Message))
        {
            return string.Format(rule.Message, memberName);
        }

        var formatArg = GetFormatValue(attr);
        var culture = rule?.Culture ?? options.CommonCulture ?? CultureInfo.CurrentCulture;
        var resourceType = rule?.ResourceType ?? attr.ErrorMessageResourceType ?? options.CommonResourceType;
        var useConventionalKeys = rule?.UseConventionalKeyFallback ?? options.UseConventionalKeys;

        var resourceKey = rule?.ResourceKey // Give priority to the rule's resource key;

            // then give a chance to the attribute's resources;
            ?? (attr.ErrorMessageResourceType != null && !string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName) ? attr.ErrorMessageResourceName : null)

            // fall back to conventional keys; otherwise, use the error message;
            ?? (useConventionalKeys ? GetConventionalKey(memberName, attr) : attr.ErrorMessage ?? attr.ErrorMessageResourceName)

            // and finally the empty string.
            ?? string.Empty;

        // 2️ Attempt resolution using IStringLocalizer
        if (rule is not null && !string.IsNullOrWhiteSpace(resourceKey))
        {
            if (TryResolveFromLocalizer(resourceKey, resourceType ?? declaringType, out var resolved))
            {
                return string.Format(culture, resolved, memberName, formatArg);
            }
        }
        else if (useConventionalKeys && resourceType is not null)
        {
            if (TryResolveFromLocalizer(resourceKey, resourceType, out var resolved))
            {
                return string.Format(culture, resolved, memberName, formatArg);
            }
        }

        // 3️ Legacy ResourceManager-based lookup as a fallback
        if (useConventionalKeys && resourceType is not null &&
        !string.IsNullOrWhiteSpace(resourceKey))
        {
            if (TryResolveFromResource(resourceType, resourceKey, culture, formatArg, out var resolved))
            {
                return string.Format(culture, resolved, memberName, formatArg);
            }
        }

        // 4️ Convention fallback via [ValidationResource]
        if (useConventionalKeys && declaringType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType is { } fallbackType)
        {
            if (TryResolveFromResource(fallbackType, resourceKey, culture, formatArg, out var resolvedFromConvention))
                return resolvedFromConvention;
        }

        // 5️ Rule-level fallback message
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.FallbackMessage))
            return rule.FallbackMessage;

        // 6 Inline message or final fallback
        string? message = null;
        try { message = attr.FormatErrorMessage(memberName); }
        catch { }

        return !string.IsNullOrWhiteSpace(message)
            ? message
            : $"Invalid value for {memberName}";
    }

    /// <summary>
    /// Attempts to resolve a localized error message from a specified resource type and key.
    /// </summary>
    /// <param name="resourceType">The resource class containing static message properties.</param>
    /// <param name="resourceKey">The property name to retrieve.</param>
    /// <param name="culture">
    /// Optional: A specific <see cref="CultureInfo"/> to format the resolved message.
    /// If <c>null</c>, defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </param>
    /// <param name="formatArg">Optional: An argument used to format the resolved message string.</param>
    /// <param name="message">
    /// When this method returns, contains the resolved and formatted message if found;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if resolution succeeded and a non-null message was returned;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool TryResolveFromResource(
        Type resourceType,
        string resourceKey,
        CultureInfo? culture,
        object? formatArg,
        [NotNullWhen(true)] out string? message)
    {
        message = null;

        if (resourceType is null || string.IsNullOrWhiteSpace(resourceKey))
            return false;

        var raw = GetResourceValue(resourceType, resourceKey);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        try
        {
            var formatter = culture ?? CultureInfo.CurrentCulture;
            message = FormatMessage(formatter, raw, formatArg);

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
    /// Attempts to resolve a localized message from a specified resource type and key using <see cref="IStringLocalizer"/>.
    /// </summary>
    /// <param name="resourceKey"></param>
    /// <param name="resourceType"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual bool TryResolveFromLocalizer(
        string resourceKey,
        Type resourceType,
        [NotNullWhen(true)] out string? message)
    {
        message = null;
        var localizer = localizerFactory.Create(resourceType);

        //if (localizer is null) return false;

        var localizedString = localizer?[resourceKey];

        if (localizedString is null || localizedString.ResourceNotFound)
        {
            return false;
        }

        message = localizedString.Value;
        return true;
    }

    /// <summary>
    /// Retrieves the value of a localized resource key exposed as a static property or method 
    /// from a resource class, typically generated from a .resx file.
    /// </summary>
    /// <param name="type">The resource type (e.g. <c>ValidationMessages</c>) containing the key.</param>
    /// <param name="key">The name of the static member to retrieve (e.g. <c>"Email_Required"</c>).</param>
    /// <returns>
    /// The resolved localized string, or <c>null</c> if the key does not exist or retrieval fails.
    /// </returns>
    protected internal static string? GetResourceValue(Type type, string key)
    {
        var member = type.GetMember(key,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault();

        return member switch
        {
            PropertyInfo prop => prop.GetValue(null)?.ToString(),
            FieldInfo field => field.GetValue(null)?.ToString(),
            MethodInfo method when method.GetParameters().Length == 0 =>
                method.Invoke(null, null)?.ToString(),
            _ => null
        };
    }

    /// <summary>
    /// Extracts contextual formatting data from common <see cref="ValidationAttribute"/> types, 
    /// which can be injected into error messages as format arguments (e.g. min/max lengths).
    /// </summary>
    /// <param name="attr">The validation attribute instance to inspect.</param>
    /// <returns>
    /// An object representing the format argument(s), such as an integer, array, or pattern string. 
    /// Returns <c>null</c> if the attribute type is unsupported or lacks relevant data.
    /// </returns>
    protected static object? GetFormatValue(ValidationAttribute attr)
    {
        return attr switch
        {
            ExactLengthAttribute m => m.MinimumLength,
            Length2Attribute m => m.MinimumLength > 0
                ? new[] { m.MinimumLength, m.MaximumLength }
                : m.MaximumLength,
            MinLengthAttribute m => m.Length,
            MaxLengthAttribute m => m.Length,
            StringLengthAttribute s => s.MinimumLength > 0
                ? new[] { s.MinimumLength, s.MaximumLength }
                : s.MaximumLength,
            RangeAttribute r => new[] { r.Minimum, r.Maximum },
            RegularExpressionAttribute r => r.Pattern,
            Compare2Attribute c2 => c2.OtherProperty, // for "must match {0}" style messages
            CompareAttribute c => c.OtherProperty, // for "must match {0}" style messages
            CreditCardAttribute => "credit-card",   // semantic format type
            EmailAddressAttribute => "email",         // useful for diagnostics or fallback tagging
            PhoneAttribute => "phone",         // helps with custom formatting or error hints
            UrlAttribute => "url",           // gives context to string-based formats
            FileExtensionsAttribute f => f.Extensions,    // could return string.Join(", ", f.Extensions)
            RequiredAttribute => "required",      // placeholder-friendly (e.g. "{0} is required")
            EqualAttribute e => e.Expected, // for "must equal {0}" style messages
            NotEqualAttribute e => e.Unexpected, // for "must not equal {0}" style messages
            EmptyAttribute => "empty",
            NotEmptyAttribute => "not empty",
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
