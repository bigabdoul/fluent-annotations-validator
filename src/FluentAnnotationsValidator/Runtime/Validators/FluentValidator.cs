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
    public virtual FluentValidationResult Validate(T instance) => Validate(instance, throwWhenNoRules: true);

    /// <summary>
    /// Validates the specified instance, optionally throwing an exception 
    /// when no rules were configured for the type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="throwWhenNoRules">
    /// <see langword="true"/> to throw an exception if no rules were configured 
    /// for <typeparamref name="T"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    /// <exception cref="InvalidOperationException">When no rules were configured for the type <typeparamref name="T"/>.</exception>
    public virtual FluentValidationResult Validate(T instance, bool throwWhenNoRules)
    {
        var enumeratedRules = options.EnumerateRules<T>();

        if (enumeratedRules.Count == 0)
        {
            if (throwWhenNoRules)
                throw new InvalidOperationException($"No rules found for the type {typeof(T).Name}. " +
                    $"Are you sure you invoked IValidationTypeConfigurator<{typeof(T).Name}>.Build() " +
                    "before calling this method?");

            return new();
        }

        var failures = new List<FluentValidationFailure>();

        foreach (var (member, rules) in enumeratedRules)
        {
            if (rules.Count == 0) continue;

            foreach (var error in rules.Validate(typeof(T), instance!, member, resolver))
            {
                failures.Add(new(error));
            }
        }

        return new(failures);
    }

    /// <inheritdoc cref="IFluentValidator.Validate(ValidationContext)"/>
    public virtual FluentValidationResult Validate(ValidationContext context)
        => Validate((T)context.ObjectInstance);

    /// <inheritdoc cref="IFluentValidator{T}.ValidateAsync(T, CancellationToken)"/>
    public virtual Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) =>
        Task.FromResult(Validate(instance));

    /// <inheritdoc cref="IFluentValidator.ValidateAsync(ValidationContext, CancellationToken)"/>
    public virtual Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = default) =>
        Task.FromResult(Validate(context));
    
    /// <inheritdoc cref="IFluentValidator.CanValidateInstancesOfType(Type)"/>
    public virtual bool CanValidateInstancesOfType(Type type) => TypeUtils.IsAssignableFrom(typeof(T), type);
}
