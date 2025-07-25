﻿using System.Globalization;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines the contract for configuring conditional validation rules
/// in a fluent and type-safe manner.
/// </summary>
public interface IValidationConfigurator
{
    /// <summary>
    /// Sets the common resource type for message resolutions that will 
    /// be applied to all models to configure via <see cref="For{T}"/>.
    /// </summary>
    /// <typeparam name="TResource">The type of the localized resource to use.</typeparam>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationConfigurator WithValidationResource<TResource>();

    /// <summary>
    /// Sets the common resource type for message resolutions that will 
    /// be replied to all models to configure via <see cref="For{T}"/>.
    /// </summary>
    /// <param name="resourceType">The type of the localized resource to use. Can be null.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationConfigurator WithValidationResource(Type? resourceType);

    /// <summary>
    /// Sets the common culture information for message resolutions that will
    /// be applied to all models to configure via <see cref="For{T}"/>.
    /// </summary>
    /// <param name="culture">The culture to apply. Can be null.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationConfigurator WithCulture(CultureInfo? culture);

    /// <summary>
    /// Begins configuring validation rules for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type to apply validation rules to.</typeparam>
    /// <returns>
    /// An <see cref="IValidationTypeConfigurator{T}"/> for defining rules specific to <typeparamref name="T"/>.
    /// </returns>
    IValidationTypeConfigurator<T> For<T>();

    /// <summary>
    /// Finalizes and applies all registered validation configurations.
    /// </summary>
    /// <remarks>
    /// This method is typically called once after all conditional rules have been defined.
    /// It ensures the options are properly registered in the DI container.
    /// </remarks>
    void Build();
}

