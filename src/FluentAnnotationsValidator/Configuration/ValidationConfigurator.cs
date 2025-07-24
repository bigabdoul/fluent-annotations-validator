using FluentAnnotationsValidator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Provides a fluent API for registering conditional validation rules and behaviors
/// into the application’s service collection.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationConfigurator"/> class
/// with the specified service collection.
/// </remarks>
/// <param name="services">The DI <see cref="IServiceCollection"/> used for registration.</param>
public class ValidationConfigurator(ValidationBehaviorOptions options) : IValidationConfigurator
{
    private readonly List<Action<ValidationBehaviorOptions>> _registrations = [];

    public ValidationBehaviorOptions Options => options;

    /// <summary>
    /// Begins configuring conditional validation rules for a specific model type.
    /// </summary>
    /// <typeparam name="T">The model type to configure validation rules for.</typeparam>
    /// <returns>A <see cref="ValidationTypeConfigurator{T}"/> to define rules for the specified type.</returns>
    public ValidationTypeConfigurator<T> For<T>()
        => new(this);

    /// <summary>
    /// Registers a configuration delegate that modifies the <see cref="ValidationBehaviorOptions"/>.
    /// </summary>
    /// <param name="config">The configuration action to apply.</param>
    public void Register(Action<ValidationBehaviorOptions> config)
        => _registrations.Add(config);

    /// <summary>
    /// Applies all registered validation configurations to the service collection.
    /// Should be called after all rules have been configured.
    /// </summary>
    public void Build()
    {
        foreach (var action in _registrations)
            action(options);
    }

    /// <summary>
    /// Begins configuring conditional validation rules for a specific model type.
    /// (Explicit interface implementation version.)
    /// </summary>
    /// <typeparam name="T">The model type to configure validation rules for.</typeparam>
    /// <returns>An <see cref="IValidationTypeConfigurator{T}"/> to define rules for the specified type.</returns>
    IValidationTypeConfigurator<T> IValidationConfigurator.For<T>() 
        => For<T>();
}


