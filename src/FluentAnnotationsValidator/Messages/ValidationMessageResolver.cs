using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
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
    /// Internal logic to resolve validation messages from conditional rules, resource attributes, or inline strings.
    /// </summary>
    /// <param name="propertyInfo">The property metadata.</param>
    /// <param name="attr">The validation attribute.</param>
    /// <param name="rule">Optional conditional rule metadata.</param>
    /// <returns>Resolved error message string or null.</returns>
    private static string? ResolveMessageInternal(PropertyValidationInfo propertyInfo, ValidationAttribute attr, ConditionalValidationRule? rule)
    {
        var modelType = propertyInfo.TargetModelType;
        var propertyName = propertyInfo.Property.Name;

        // 1️ Rule-based override: message or resource
        if (rule is not null)
        {
            if (!string.IsNullOrWhiteSpace(rule.Message))
                return rule.Message;

            if (!string.IsNullOrWhiteSpace(rule.ResourceKey) && rule.ResourceType is not null)
            {
                var prop = rule.ResourceType.GetProperty(rule.ResourceKey,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var raw = prop?.GetValue(null)?.ToString();
                if (!string.IsNullOrWhiteSpace(raw))
                    return Format(raw, attr, propertyName);
            }
        }

        // 2️ Attribute-based resource lookup
        if (!string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var explicitType = attr.ErrorMessageResourceType
                ?? modelType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

            if (explicitType is not null)
            {
                var prop = explicitType.GetProperty(attr.ErrorMessageResourceName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var raw = prop?.GetValue(null)?.ToString();
                if (!string.IsNullOrWhiteSpace(raw))
                    return Format(raw, attr, propertyName);
            }
        }

        // 3️ Convention fallback via ValidationResourceAttribute
        var fallbackType = modelType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;
        if (fallbackType is not null)
        {
            var key = GetConventionalKey(propertyName, attr);
            var prop = fallbackType.GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            var raw = prop?.GetValue(null)?.ToString();
            if (!string.IsNullOrWhiteSpace(raw))
                return Format(raw, attr, propertyName);
        }

        // 4️ Inline message fallback
        return !string.IsNullOrWhiteSpace(attr.ErrorMessage)
            ? attr.FormatErrorMessage(propertyName)
            : $"Invalid value for {propertyName}";
    }

    /// <summary>
    /// Formats a raw message with the best available argument.
    /// </summary>
    /// <param name="raw">The raw error message string.</param>
    /// <param name="attr">The validation attribute.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>Formatted string.</returns>
    private static string Format(string raw, ValidationAttribute attr, string propertyName)
    {
        var arg = GetFormatValue(attr);
        return arg != null
            ? string.Format(raw, arg)
            : string.Format(raw, propertyName);
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
