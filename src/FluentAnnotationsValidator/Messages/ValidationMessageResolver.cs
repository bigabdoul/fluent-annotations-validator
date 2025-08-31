using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Metadata;
using Microsoft.Extensions.Localization;
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
public class ValidationMessageResolver(ValidationBehaviorOptions options, IStringLocalizerFactory localizerFactory) : IValidationMessageResolver
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
    /// Returns <see langword="null"/> if no message can be resolved.
    /// </returns>
    public virtual string? ResolveMessage(Type declaringType, string memberName, ValidationAttribute attr, ConditionalValidationRule? rule = null)
    {
        // 1️ Rule-based explicit message override
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.Message))
        {
            return string.Format(rule.Message, memberName);
        }

        var formatArg = GetFormatValue(attr);
        var culture = rule?.Culture ?? options.SharedCulture ?? CultureInfo.CurrentCulture;

        var attrErrorMessageResourceType = attr.ErrorMessageResourceType ?? declaringType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

        var resourceType = rule?.ResourceType ?? attrErrorMessageResourceType ?? options.SharedResourceType;
        var useConventionalKeys = rule?.UseConventionalKeys ?? options.UseConventionalKeys;

        var resourceKey = rule?.ResourceKey // Give priority to the rule's resource key;

            // then give a chance to the attribute's resources;
            ?? (attrErrorMessageResourceType != null && !string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName) ? attr.ErrorMessageResourceName : null)

            // fall back to conventional keys; otherwise, use the error message;
            ?? (useConventionalKeys ? options.ConventionalKeyGetter?.Invoke(declaringType, memberName, attr) ?? declaringType.GetConventionalKey(memberName, attr) : attr.ErrorMessage ?? attr.ErrorMessageResourceName)

            // and finally the empty string.
            ?? string.Empty;

        // 2️ Attempt resolution using IStringLocalizer
        if (rule is not null && !string.IsNullOrWhiteSpace(resourceKey))
        {
            if (TryResolveFromLocalizer(localizerFactory, resourceKey, resourceType ?? declaringType, culture, formatArg, out var resolved))
                return resolved;
        }
        else if (useConventionalKeys && resourceType is not null)
        {
            if (TryResolveFromLocalizer(localizerFactory, resourceKey, resourceType, culture, formatArg, out var resolved))
                return resolved;
        }

        // 3️ Legacy ResourceManager-based lookup as a fallback
        if (useConventionalKeys && resourceType is not null &&
        !string.IsNullOrWhiteSpace(resourceKey))
        {
            if (TryResolveFromResource(resourceType, resourceKey, culture, formatArg, out var resolved))
                return resolved;
        }

        // 4️ Convention fallback via [ValidationResource]
        if (useConventionalKeys && attrErrorMessageResourceType != null)
        {
            if (TryResolveFromResource(attrErrorMessageResourceType, resourceKey, culture, formatArg, out var resolvedFromConvention))
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
    /// If <see langword="null"/>, defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </param>
    /// <param name="formatArg">Optional: An argument used to format the resolved message string.</param>
    /// <param name="message">
    /// When this method returns, contains the resolved and formatted message if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if resolution succeeded and a non-null message was returned;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected static bool TryResolveFromResource(Type resourceType,
    string resourceKey, CultureInfo? culture, object? formatArg,
    [NotNullWhen(true)] out string? message)
    {
        message = null;

        if (resourceType is null || string.IsNullOrWhiteSpace(resourceKey))
            return false;

        culture ??= CultureInfo.CurrentCulture;
        
        var raw = resourceType.GetResourceValue(resourceKey, culture);
        
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
    /// Attempts to resolve a localized message from a specified resource 
    /// type and key using <see cref="IStringLocalizerFactory"/>.
    /// </summary>
    /// <param name="localizerFactory">The factory used to resolve the localized message.</param>
    /// <param name="resourceKey">The resource key to retrieve.</param>
    /// <param name="resourceType">The resource type to look up.</param>
    /// <param name="message">Returns the localized message if the look up was successful.</param>
    /// <param name="culture">
    /// Optional: A specific <see cref="CultureInfo"/> to look up the resource key.
    /// If <see langword="null"/>, defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if resolution succeeded and a non-null message was returned;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The method first attempts to resolve the specified <paramref name="resourceType"/>
    /// and <paramref name="resourceKey"/> by calling the method 
    /// <see cref="TryResolveFromResource(Type, string, CultureInfo?, object?, out string?)"/>.
    /// </remarks>
    protected static bool TryResolveFromLocalizer(IStringLocalizerFactory localizerFactory,
    string resourceKey, Type resourceType, CultureInfo? culture,
    object? formatArg, [NotNullWhen(true)] out string? message)
    {
        if (TryResolveFromResource(resourceType, resourceKey, culture, formatArg, out message))
            return true;

        var localizer = localizerFactory.Create(resourceType);
        var localizedString = localizer?[resourceKey];

        if (localizedString is null || localizedString.ResourceNotFound)
        {
            return false;
        }

        message = localizedString.Value;
        return true;
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
    /// Returns <see langword="null"/> if the attribute type is unsupported or lacks relevant data.
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
}
