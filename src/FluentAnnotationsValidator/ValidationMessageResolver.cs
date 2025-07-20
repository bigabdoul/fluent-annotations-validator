using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

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
        var modelType = propertyInfo.TargetModelType;
        var propertyName = propertyInfo.Property.Name;
        
        // 1️ Explicit resource name + type wins
        if (!string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var explicitType = attr.ErrorMessageResourceType
                ?? modelType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;

            if (explicitType != null)
            {
                var prop = explicitType.GetProperty(attr.ErrorMessageResourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var rawMessage = prop?.GetValue(null)?.ToString();

                if (!string.IsNullOrWhiteSpace(rawMessage))
                {
                    var formatArg = GetFormatValue(attr);
                    return formatArg != null
                        ? string.Format(rawMessage, formatArg)
                        : string.Format(rawMessage, propertyName);
                }
            }
        }

        // 2️ Convention fallback: Property.Attribute
        var fallbackType = modelType.GetCustomAttribute<ValidationResourceAttribute>()?.ErrorMessageResourceType;
        if (fallbackType != null)
        {
            var key = GetConventionalKey(propertyName, attr);
            var prop = fallbackType.GetProperty(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var rawMessage = prop?.GetValue(null)?.ToString();

            if (!string.IsNullOrWhiteSpace(rawMessage))
            {
                var formatArg = GetFormatValue(attr);
                return formatArg != null
                    ? string.Format(rawMessage, formatArg)
                    : string.Format(rawMessage, propertyName);
            }
        }

        // 3️ Inline message or fallback
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
