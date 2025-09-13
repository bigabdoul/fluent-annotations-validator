using System.Globalization;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines the contract for configuring validation rules
/// in a fluent and type-safe manner.
/// </summary>
public interface IFluentTypeValidatorRoot
{
    /// <summary>
    /// Sets the default resource type for localization lookups in this configuration chain.
    /// </summary>
    /// <typeparam name="TResource">The type parameter of the validation resource type to use.</typeparam>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidatorRoot WithValidationResource<TResource>();

    /// <summary>
    /// Sets the default resource type for localization lookups in this configuration chain.
    /// </summary>
    /// <param name="resourceType">The validation resource type to use. Can be null.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidatorRoot WithValidationResource(Type? resourceType);

    /// <summary>
    /// Sets the culture used during error message resolution.
    /// </summary>
    /// <param name="culture">The culture information to set.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidatorRoot WithCulture(CultureInfo culture);

    /// <summary>
    /// Begins configuring validation rules for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type to apply validation rules to.</typeparam>
    /// <returns>
    /// An <see cref="IFluentTypeValidator{T}"/> for defining rules specific to <typeparamref name="T"/>.
    /// </returns>
    IFluentTypeValidator<T> For<T>();
}
