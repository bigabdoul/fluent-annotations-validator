using FluentAnnotationsValidator.Abstractions;
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
	/// <returns>An initialized <see cref="FluentValidationResult"/> object contains any validation failures.</returns>
	FluentValidationResult Validate(ValidationContext context);

    /// <summary>
    /// Validates the specified instance asynchronously.
    /// </summary>
    /// <param name="context">A ValidationContext</param>
    /// <param name="cancellation">An object that propagates notification that operations should be canceled.</param>
    /// <returns>A task whose result contains an initialized <see cref="FluentValidationResult"/> object contains any validation failures.</returns>
    Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = default);

    /// <summary>
    /// Checks to see whether the validator can validate objects of the specified type
    /// </summary>
    bool CanValidateInstancesOfType(Type type);

    /// <summary>
    /// Sets an alternative registry for rules retrieval.
    /// </summary>
    /// <param name="value">The rule registry to use.</param>
    void SetRuleRegistry(IRuleRegistry value);

    /// <summary>
    /// Sets an alternative validation message resolver.
    /// </summary>
    /// <param name="value">The validation message resolver to use.</param>
    void SetMessageResolver(IValidationMessageResolver value);
}

/// <summary>
/// Defines a contract for validators that can validate instances of a specific type.
/// </summary>
/// <typeparam name="T">The type of the object instance to validate.</typeparam>
public interface IFluentValidator<T> : IFluentValidator
{
    /// <summary>
	/// Validates the specified instance.
	/// </summary>
	/// <param name="instance">The instance to validate</param>
	/// <returns>An initialized <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
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
    /// <returns>An initialized <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
	FluentValidationResult Validate(T instance, bool throwWhenNoRules);

    /// <summary>
    /// Validate the specified instance asynchronously
    /// </summary>
    /// <param name="instance">The instance to validate</param>
    /// <param name="cancellation">An object that propagates notification that operations should be canceled.</param>
    /// <returns>A task whose result contains an initialized <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);
}
