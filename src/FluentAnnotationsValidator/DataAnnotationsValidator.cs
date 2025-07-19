using FluentValidation;
using System.ComponentModel.DataAnnotations;

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
                            var message = attr.ResolveMessage(prop.Property.Name, typeof(T));
                            ctx.AddFailure(prop.Property.Name, message);
                        }
                    });
            }
        }
    }
}
