using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentValidation;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;

namespace FluentAnnotationsValidator.Runtime.Validators;

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
    public DataAnnotationsValidator(IValidationMessageResolver resolver, IOptions<ValidationBehaviorOptions> options)
    {
        var metadata = ValidationMetadataCache.Get(typeof(T));
        var config = options.Value;

        foreach (var prop in metadata)
        {
            foreach (var attr in prop.Attributes)
            {
                _ = config.TryGet(typeof(T), prop.Property.Name, out var rule);

                RuleFor(model => prop.Property.GetValue(model))
                    .Custom((value, ctx) =>
                    {
                        if (!attr.IsValid(value))
                        {
                            var message = resolver.ResolveMessage(prop, attr, rule);
                            var failure = new ValidationFailure(prop.Property.Name, message, value);
                            ctx.AddFailure(failure);
                        }
                    })
                    .When(model => model is not null && (rule?.Predicate(model) ?? true));
            }
        }
    }
}
