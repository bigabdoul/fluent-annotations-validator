using System.Globalization;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines the contract for configuring conditional validation rules
/// in a fluent and type-safe manner.
/// </summary>
public interface IValidationConfigurator
{
    /// <summary>
    /// Sets the default resource type for localization lookups in this configuration chain.
    /// </summary>
    /// <typeparam name="TResource">The type parameter of the validation resource type to use.</typeparam>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationConfigurator WithValidationResource<TResource>();

    /// <summary>
    /// Sets the default resource type for localization lookups in this configuration chain.
    /// </summary>
    /// <param name="resourceType">The validation resource type to use. Can be null.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationConfigurator WithValidationResource(Type? resourceType);

    /// <summary>
    /// Sets the culture used during error message resolution.
    /// </summary>
    /// <param name="culture">The culture information to set.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationConfigurator WithCulture(CultureInfo culture);

    /// <summary>
    /// Begins configuring validation rules for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type to apply validation rules to.</typeparam>
    /// <returns>
    /// An <see cref="IValidationTypeConfigurator{T}"/> for defining rules specific to <typeparamref name="T"/>.
    /// </returns>
    IValidationTypeConfigurator<T> For<T>();
}

