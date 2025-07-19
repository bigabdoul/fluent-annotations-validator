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

    private static string ResolveErrorMessage(ValidationAttribute attr, string propertyName)
    {
        if (attr.ErrorMessageResourceType != null && !string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var prop = attr.ErrorMessageResourceType.GetProperty(attr.ErrorMessageResourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var val = prop?.GetValue(null)?.ToString();
            return string.IsNullOrWhiteSpace(val)
                ? $"Invalid value for {propertyName}"
                : string.Format(val, propertyName);
        }

        return !string.IsNullOrWhiteSpace(attr.ErrorMessage) 
            ? attr.FormatErrorMessage(propertyName) 
            : $"Invalid value for {propertyName}";
    }
}
