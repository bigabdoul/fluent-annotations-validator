using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Results;
using FluentValidation;
using FluentValidation.Results;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// Validates an object using [ValidationAttribute] rules mapped via the rule registry.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public sealed class DataAnnotationsValidator<T>(ValidationBehaviorOptions options, IValidationMessageResolver resolver) : IValidator<T>
{
    /// <inheritdoc cref="IValidator{T}.Validate(T)"/>
    public ValidationResult Validate(T instance)
    {
        var failures = new List<ValidationFailure>();

        foreach (var (member, rules) in options.EnumerateRules<T>())
        {
            var errors = ValidationResultAggregator.Evaluate(typeof(T), instance!, member, rules, resolver);

            foreach (var error in errors)
            {
                failures.Add(new ValidationFailure(error.Member.Name, error.Message));
            }
        }

        return new ValidationResult(failures);
    }

    /// <inheritdoc cref="IValidator{T}.ValidateAsync(T, CancellationToken)"/>
    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) =>
            Task.FromResult(Validate(instance));

    /// <inheritdoc cref="IValidator{T}.Validate(T)"/>
    public ValidationResult Validate(IValidationContext context)
    {
        if (context is ValidationContext<T> typedContext)
            return Validate(typedContext.InstanceToValidate);

        throw new ArgumentException($"Invalid context type: {context.GetType().Name}", nameof(context));
    }

    /// <inheritdoc cref="IValidator.ValidateAsync(IValidationContext, CancellationToken)"/>
    public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default) =>
        Task.FromResult(Validate(context));

    /// <inheritdoc cref="IValidator.CreateDescriptor"/>
    public IValidatorDescriptor CreateDescriptor() => new FluentValidatorDescriptor();

    /// <inheritdoc cref="IValidator.CanValidateInstancesOfType(Type)"/>
    public bool CanValidateInstancesOfType(Type type) => typeof(T).IsAssignableFrom(type);
}
