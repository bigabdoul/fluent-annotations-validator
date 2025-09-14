using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Results;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Base class for all FluentValidation attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public abstract class FluentValidationAttribute : ValidationAttribute, IValidationResult
{
    private IServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected IServiceProvider ServiceProvider =>
        _serviceProvider ??= FluentAnnotationsBuilder.Default?.Services.BuildServiceProvider()
        ?? throw new InvalidOperationException("No service provider available.");

    /// <summary>
    /// Gets or sets the rule registry for validator resolution.
    /// </summary>
    public IRuleRegistry? RuleRegistry { get; set; }

    /// <summary>
    /// Gets the list of validation failures.
    /// </summary>
    public List<FluentValidationFailure> Errors { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationAttribute"/> class with no error message.
    /// </summary>
    public FluentValidationAttribute() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationAttribute"/> class with a specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message to use when validation fails.</param>
    protected FluentValidationAttribute(string errorMessage) : base(errorMessage) { }

    /// <summary>
    /// Optional message resolver to use for resolving validation messages.
    /// </summary>
    public virtual IValidationMessageResolver? MessageResolver { get; set; }

    /// <summary>
    /// Gets or sets the validation rule associated with this attribute.
    /// The value of this property is intended to be passed to message resolver.
    /// </summary>
    public virtual IValidationRule? Rule { get; set; }

    /// <summary>
    /// Instantiates a type-specific <see cref="IFluentValidator"/> using the configured <see cref="ServiceProvider"/>.
    /// </summary>
    /// <param name="objectType">The target type for which a validator should be created.</param>
    /// <returns>
    /// A configured <see cref="IFluentValidator"/> instance capable of validating objects of the specified type.
    /// </returns>
    protected IFluentValidator CreateValidator(Type objectType)
    {
        var validatorType = typeof(IFluentValidator<>).MakeGenericType(objectType);
        var validator = (IFluentValidator)ServiceProvider.GetRequiredService(validatorType);

        if (RuleRegistry != null)
            validator.SetRuleRegistry(RuleRegistry);

        if (MessageResolver != null)
            validator.SetMessageResolver(MessageResolver!);

        return validator;
    }

    /// <summary>
    /// Generates a <see cref="ValidationResult"/> based on a list of validation errors.
    /// </summary>
    /// <param name="errors">A list of <see cref="FluentValidationFailure"/> objects containing validation errors.</param>
    /// <returns>
    /// <para>
    /// <see cref = "ValidationResult.Success" /> if the list of errors is empty.
    /// </para>
    /// <para>
    /// Otherwise, a new <see cref = "ValidationResult" /> with a summary message
    /// and the list of errors added to the class's <see cref="Errors"/> collection.
    /// </para>
    /// </returns>
    protected virtual ValidationResult? GetValidationResult(List<FluentValidationFailure> errors)
    {
        if (errors.Count == 0) return ValidationResult.Success;
        Errors.AddRange(errors);
        return new ValidationResult($"Validation errors occurred: {errors.Count}. See Errors for details.");
    }
}
