using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Results;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// A fluent validator that performs validation on an object of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// This class serves as the core entry point for the validation process. It uses the
/// configured rules from <see cref="ValidationBehaviorOptions"/> and resolves error
/// messages using the <see cref="IValidationMessageResolver"/>.
/// </remarks>
/// <param name="options">The configured validation behavior options for the application.</param>
/// <param name="resolver">The service responsible for resolving validation error messages.</param>
/// <typeparam name="T">The type of the object instance to be validated.</typeparam>
public class FluentValidator<T>(ValidationBehaviorOptions options, IValidationMessageResolver resolver) : IFluentValidator<T>
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no rules were configured for the type <typeparamref name="T"/>.
    /// </exception>
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
    public virtual FluentValidationResult Validate(T instance, bool throwWhenNoRules)
    {
        var enumeratedRules = options.EnumerateRules<T>();

        if (enumeratedRules.Count == 0)
        {
            if (throwWhenNoRules)
            {
                throw new InvalidOperationException(
                    $"No rules found for the type {typeof(T).Name}. " +
                    $"Are you sure you invoked IValidationTypeConfigurator<{typeof(T).Name}>.Build() " +
                    "before calling this method?");
            }

            return new();
        }

        var failures = new List<FluentValidationFailure>();

        foreach (var (member, rules) in enumeratedRules)
        {
            foreach (var error in rules.Validate(typeof(T), instance!, member, resolver))
            {
                failures.Add(new(error));
            }
        }

        return new(failures);
    }

    /// <inheritdoc />
    public virtual FluentValidationResult Validate(ValidationContext context)
        => Validate((T)context.ObjectInstance);

    /// <summary>
    /// Asynchronously validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to a <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    public virtual async Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        var enumeratedRules = options.EnumerateRules<T>();

        if (enumeratedRules.Count == 0)
        {
            return new();
        }

        var failures = new List<FluentValidationFailure>();

        foreach (var (member, rules) in enumeratedRules)
        {
            foreach (var error in await rules.ValidateAsync(typeof(T), instance!, member, resolver, cancellationToken))
            {
                failures.Add(new(error));
            }
        }

        return new(failures);
    }

    /// <inheritdoc />
    public virtual Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellationToken = default)
        => ValidateAsync((T)context.ObjectInstance, cancellationToken);

    /// <summary>
    /// Determines whether the validator can validate instances of the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the validator can validate the type; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool CanValidateInstancesOfType(Type type) => TypeUtils.IsAssignableFrom(typeof(T), type);
}
