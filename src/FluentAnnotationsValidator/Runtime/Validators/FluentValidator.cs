using FluentAnnotationsValidator.Results;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// Validates an object using a FluentValidation validator.
/// </summary>
/// <typeparam name="T">The model type to validate.</typeparam>
/// <param name="validator">
/// The FluentValidation validator instance to use for validation.
/// </param>
public class FluentValidator<T>(IValidator<T> validator) : IFluentValidator<T>
{
    /// <summary>
    /// Validates an instance of type <typeparamref name="T"/> 
    /// using the provided FluentValidation validator.
    /// </summary>
    /// <param name="instance">
    /// The instance of type <typeparamref name="T"/> to validate.
    /// </param>
    /// <returns>
    /// A <see cref="FluentValidationResult"/> containing validation errors,
    /// </returns>
    public FluentValidationResult Validate(T instance)
    {
        var result = validator.Validate(instance);

        if (!result.IsValid)
        {
            var validationResult = new FluentValidationResult();

            foreach (var err in result.Errors)
            {
                validationResult.Errors.Add(new FluentValidationFailure(err));
            }

            return validationResult;
        }

        return FluentValidationResult.Success;
    }

    /// <inheritdoc cref="IFluentValidator.Validate(ValidationContext)"/>
    public virtual FluentValidationResult Validate(ValidationContext context)
        => Validate((T)context.ObjectInstance);

    /// <inheritdoc cref="IFluentValidator{T}.ValidateAsync(T, CancellationToken)"/>
    public virtual Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default)
        => Task.FromResult(Validate(instance));


    /// <inheritdoc cref="IFluentValidator.ValidateAsync(ValidationContext, CancellationToken)"/>
    public virtual Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = default)
        => Task.FromResult(Validate((T)context.ObjectInstance));

    /// <inheritdoc cref="IFluentValidator.CanValidateInstancesOfType(Type)"/>/>
    public bool CanValidateInstancesOfType(Type type)
        => validator.CanValidateInstancesOfType(type);
}
