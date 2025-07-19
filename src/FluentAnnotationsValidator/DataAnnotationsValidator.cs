using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

/// <summary>
/// A FluentValidation adapter that inspects <see cref="ValidationAttribute"/> metadata on model properties
/// and dynamically applies equivalent FluentValidation rules at runtime.
/// Supports error message resolution via FormatErrorMessage, resource keys, and localization.
/// </summary>
/// <typeparam name="T">The model type to validate.</typeparam>
public class DataAnnotationsValidator<T> : AbstractValidator<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataAnnotationsValidator{T}"/> class.
    /// </summary>
    public DataAnnotationsValidator()
    {
        var metadata = ValidationMetadataCache.Get(typeof(T));

        foreach (var prop in metadata)
        {
            foreach (var attr in prop.Attributes)
            {
                RuleFor(model => prop.Property.GetValue(model))
                    .Custom((value, ctx) =>
                    {
                        if (!attr.IsValid(value))
                        {
                            string message = ResolveErrorMessage(attr, prop.Property.Name);
                            ctx.AddFailure(prop.Property.Name, message);
                        }
                    });
            }
        }
    }

    private static readonly Type? _fallbackResourceType = typeof(T)
        .GetCustomAttribute<ValidationResourceAttribute>(inherit: true)
        ?.ErrorMessageResourceType;

    private static string ResolveErrorMessage(ValidationAttribute attr, string propertyName)
    {
        var resourceType = attr.ErrorMessageResourceType ?? _fallbackResourceType;

        // Prefer localization via resource provider
        if (resourceType != null && !string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var prop = resourceType.GetProperty(
                attr.ErrorMessageResourceName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            var rawMessage = prop?.GetValue(null)?.ToString();

            if (!string.IsNullOrWhiteSpace(rawMessage))
            {
                object? contextArg = GetFormatValue(attr);
                return contextArg is not null
                    ? string.Format(rawMessage, contextArg)
                    : string.Format(rawMessage, propertyName);
            }
        }

        // Use inline ErrorMessage or fallback to default
        return !string.IsNullOrWhiteSpace(attr.ErrorMessage)
            ? attr.FormatErrorMessage(propertyName)
            : $"Invalid value for {propertyName}";
    }

    private static object? GetFormatValue(ValidationAttribute attr)
    {
        return attr switch
        {
            MinLengthAttribute m => m.Length,
            MaxLengthAttribute m => m.Length,
            StringLengthAttribute s => s.MaximumLength,
            RangeAttribute r => $"{r.Minimum}–{r.Maximum}",
            RequiredAttribute => null, // Usually no format arg needed
            EmailAddressAttribute => null,
            UrlAttribute => null,
            CreditCardAttribute => null,
            RegularExpressionAttribute r => r.Pattern,
            _ => null // Leave room for future extensibility or custom types
        };
    }
}
