using FluentAnnotationsValidator.Results;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator;

/// <summary>
/// Defines a contract for validators that can validate instances of a specific type.
/// </summary>
public interface IFluentValidator
{
    /// <summary>
	/// Validates the specified instance.
	/// </summary>
	/// <param name="context">A ValidationContext</param>
	/// <returns>A FluentValidationResult object contains any validation failures.</returns>
	FluentValidationResult Validate(ValidationContext context);

    /// <summary>
    /// Validates the specified instance asynchronously.
    /// </summary>
    /// <param name="context">A ValidationContext</param>
    /// <param name="cancellation">Cancellation token</param>
    /// <returns>A FluentValidationResult object contains any validation failures.</returns>
    Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = default);

    /// <summary>
    /// Checks to see whether the validator can validate objects of the specified type
    /// </summary>
    bool CanValidateInstancesOfType(Type type);
}

/// <summary>
/// Defines a contract for validators that can validate instances of a specific type.
/// </summary>
/// <typeparam name="T">The type of the object instance to validate.</typeparam>
/// <remarks>
/// Represents a wrapper around the <see cref="IFluentValidator{T}"/>
/// defined in the FluentValidation API, and allows getting away
/// from a direct reference to the FluentAnnotationsValidator library.
/// </remarks>
public interface IFluentValidator<T> : IFluentValidator
{
    /// <summary>
	/// Validates the specified instance.
	/// </summary>
	/// <param name="instance">The instance to validate</param>
	/// <returns>A FluentValidationResult object containing any validation failures.</returns>
	FluentValidationResult Validate(T instance);

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
	FluentValidationResult Validate(T instance, bool throwWhenNoRules);

    /// <summary>
    /// Validate the specified instance asynchronously
    /// </summary>
    /// <param name="instance">The instance to validate</param>
    /// <param name="cancellation"></param>
    /// <returns>A FluentValidationResult object containing any validation failures.</returns>
    Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);
}
