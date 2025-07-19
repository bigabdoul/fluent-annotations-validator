using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentValidation;

namespace FluentAnnotationsValidator;

public class DataAnnotationsValidator<T> : AbstractValidator<T>
{
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
        if (!string.IsNullOrWhiteSpace(attr.ErrorMessage))
            return attr.FormatErrorMessage(propertyName);

        if (attr.ErrorMessageResourceType != null && !string.IsNullOrWhiteSpace(attr.ErrorMessageResourceName))
        {
            var prop = attr.ErrorMessageResourceType.GetProperty(attr.ErrorMessageResourceName, BindingFlags.Public | BindingFlags.Static);
            var val = prop?.GetValue(null)?.ToString();
            return string.IsNullOrWhiteSpace(val)
                ? $"Invalid value for {propertyName}"
                : string.Format(val, propertyName);
        }

        return $"Invalid value for {propertyName}";
    }
}
