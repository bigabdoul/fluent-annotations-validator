using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Results;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// Validates an object whose members are decorated with custom validation attributes, using rules mapped via the rule registry.
/// </summary>
public class FluentValidator<T>(ValidationBehaviorOptions options, IValidationMessageResolver resolver) : IFluentValidator<T>
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    public virtual FluentValidationResult Validate(T instance)
    {
        var failures = new List<FluentValidationFailure>();

        foreach (var (member, rules) in options.EnumerateRules<T>())
        {
            foreach (var error in rules.Validate(typeof(T), instance!, member, resolver))
            {
                failures.Add(new(error));
            }
        }

        return new(failures);
    }

    public virtual FluentValidationResult Validate(ValidationContext context)
        => Validate((T)context.ObjectInstance);

    public virtual Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) =>
        Task.FromResult(Validate(instance));

    public virtual Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = default) =>
        Task.FromResult(Validate(context));
    
    public virtual bool CanValidateInstancesOfType(Type type) => 
        TypeUtils.IsAssignableFrom(typeof(T), type);
}
