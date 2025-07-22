using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace FluentAnnotationsValidator.Messages;

/// <summary>
/// Provides mechanisms for resolving localized error messages associated with <see cref="ValidationAttribute"/> instances.
/// Supports conventional lookup, explicit resource naming, and fallback formatting strategies.
/// </summary>
public class ValidationMessageResolver : IValidationMessageResolver
{
    /// <summary>
    /// Resolves a validation error message for a given <see cref="ValidationAttribute"/>, using one of the following:
    /// <list type="number">
    ///   <item>
    ///     <description>Explicit <c>ErrorMessageResourceType</c> and <c>ErrorMessageResourceName</c> on the attribute</description>
    ///   </item>
    ///   <item>
    ///     <description><c>[ValidationResource]</c> on the model type, plus convention: <c>Property_Attribute</c></description>
    ///   </item>
    ///   <item>
    ///     <description>Inline <c>ErrorMessage</c> or default fallback message via <see cref="ValidationAttribute.FormatErrorMessage"/></description>
    ///   </item>
    /// </list>
    /// </summary>
    /// <param name="propertyInfo">The name of the property to which the attribute is applied.</param>
    /// <param name="attr">The <see cref="ValidationAttribute"/> being evaluated.</param>
    /// <returns>A formatted error message suitable for display or logging.</returns>
    public virtual string ResolveMessage(PropertyValidationInfo propertyInfo, ValidationAttribute attr)
    {
        return ResolveMessageInternal(propertyInfo, attr, rule: null)!;
    }

    /// <summary>
    /// Resolves the error message to be used for a validation failure, based on the supplied
    /// <see cref="ValidationAttribute"/>, property metadata, and optional conditional rule context.
    /// </summary>
    /// <param name="propertyInfo">
    /// The metadata container for the property being validated, including its <see cref="PropertyInfo"/> and target model type.
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
    public virtual string? ResolveMessage(PropertyValidationInfo propertyInfo, ValidationAttribute attr, ConditionalValidationRule? rule)
    {
        return ResolveMessageInternal(propertyInfo, attr, rule);
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

        var prop = resourceType.GetProperty(resourceKey,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        var raw = prop?.GetValue(null)?.ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        try
        {
            var formatter = culture ?? CultureInfo.CurrentCulture;
            message = formatArg != null
                ? string.Format(formatter, raw, formatArg)
                : raw;

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
    /// Internal logic to resolve validation messages from conditional rules, resource attributes, or inline strings.
    /// </summary>
    /// <param name="propertyInfo">The property metadata.</param>
    /// <param name="attr">The validation attribute.</param>
    /// <param name="rule">Optional conditional rule metadata.</param>
    /// <returns>Resolved error message string or null.</returns>
    protected virtual string? ResolveMessageInternal(PropertyValidationInfo propertyInfo, ValidationAttribute attr, ConditionalValidationRule? rule)
    {
        var modelType = propertyInfo.TargetModelType;
        var propertyName = propertyInfo.Property.Name;

        // 1️⃣ Rule-based explicit message override
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.Message))
            return rule.Message;

        var formatArg = GetFormatValue(attr);
        var culture = rule?.Culture ?? CultureInfo.CurrentCulture;

        // 2️⃣ Rule-based resource lookup
        if (rule is not null &&
            !string.IsNullOrWhiteSpace(rule.ResourceKey) &&
            rule.ResourceType is not null &&
            TryResolveFromResource(rule.ResourceType, rule.ResourceKey!, culture, formatArg, out var resolvedFromRule))
        {
            return resolvedFromRule;
        }

        // 3️⃣ Attribute-based explicit resource key
        if (!string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var explicitType = attr.ErrorMessageResourceType
                ?? modelType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

            if (explicitType is not null &&
                TryResolveFromResource(explicitType, attr.ErrorMessageResourceName!, culture, formatArg, out var resolvedFromAttr))
            {
                return resolvedFromAttr;
            }
        }

        // 4️⃣ Convention fallback via [ValidationResource]
        var fallbackType = modelType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

        if (fallbackType is not null && (rule?.UseConventionalKeyFallback ?? true))
        {
            var key = GetConventionalKey(propertyName, attr);

            if (TryResolveFromResource(fallbackType, key, culture, formatArg, out var resolvedFromConvention))
                return resolvedFromConvention;
        }

        // 5️⃣ Rule-level fallback message
        if (rule is not null && !string.IsNullOrWhiteSpace(rule.FallbackMessage))
            return rule.FallbackMessage;

        // 6️⃣ Inline message or final fallback
        return !string.IsNullOrWhiteSpace(attr.ErrorMessage)
            ? attr.FormatErrorMessage(propertyName)
            : $"Invalid value for {propertyName}";
    }

    private static string GetConventionalKey(string propertyName, ValidationAttribute attr)
    {
        var shortName = attr.GetType().Name.Replace("Attribute", "");
        return $"{propertyName}_{shortName}";
    }

    private static object? GetFormatValue(ValidationAttribute attr)
    {
        return attr switch
        {
            MinLengthAttribute m => m.Length,
            MaxLengthAttribute m => m.Length,
            StringLengthAttribute s => s.MaximumLength,
            RangeAttribute r => $"{r.Minimum}–{r.Maximum}",
            RegularExpressionAttribute r => r.Pattern,
            _ => null
        };
    }
}
