using System.Globalization;

namespace FluentAnnotationsValidator.Runtime;

using Core;
using Extensions;
using Interfaces;

/// <summary>
/// Provides a fluent API for registering conditional validation rules and behaviors
/// into the application’s service collection.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluentTypeValidatorRoot"/> class
/// with the specified service collection.
/// </remarks>
/// <param name="registry">The validation behavior options used during configuration.</param>
public class FluentTypeValidatorRoot(IValidationRuleGroupRegistry registry) : IFluentTypeValidatorRoot
{
    /// <summary>
    /// Begins configuring conditional validation rules for a specific model type.
    /// </summary>
    /// <typeparam name="T">The model type to configure validation rules for.</typeparam>
    /// <returns>A <see cref="FluentTypeValidator{T}"/> to define rules for the specified type.</returns>
    public virtual FluentTypeValidator<T> For<T>()
    {
        return new(this);
    }

    /// <summary>
    /// Gets the validation rule group registry.
    /// </summary>
    public IValidationRuleGroupRegistry Registry => registry;

    /// <inheritdoc cref="IFluentTypeValidatorRoot.WithValidationResource{TResource}()"/>
    public FluentTypeValidatorRoot WithValidationResource<TResource>() => WithValidationResource(typeof(TResource));

    /// <inheritdoc cref="IFluentTypeValidatorRoot.WithValidationResource(Type?)"/>
    public FluentTypeValidatorRoot WithValidationResource(Type? resourceType)
    {
        GlobalRegistry.Default.SharedResourceType = resourceType;
        AssignCultureTo(resourceType);
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidatorRoot.WithCulture(CultureInfo)"/>
    public FluentTypeValidatorRoot WithCulture(CultureInfo culture)
    {
        GlobalRegistry.Default.SharedCulture = culture;
        return this;
    }

    /// <summary>
    /// Begins configuring conditional validation rules for a specific model type.
    /// (Explicit interface implementation version.)
    /// </summary>
    /// <typeparam name="T">The model type to configure validation rules for.</typeparam>
    /// <returns>An <see cref="IFluentTypeValidator{T}"/> to define rules for the specified type.</returns>
    IFluentTypeValidator<T> IFluentTypeValidatorRoot.For<T>() => For<T>();

    private static void AssignCultureTo(Type? type)
    {
        type.TrySetResourceManagerCulture(GlobalRegistry.Default.SharedCulture, fallbackToType: true);
    }

    #region IFluentTypeValidatorInitial

    IFluentTypeValidatorRoot IFluentTypeValidatorRoot.WithValidationResource<TResource>()
        => WithValidationResource<TResource>();

    IFluentTypeValidatorRoot IFluentTypeValidatorRoot.WithValidationResource(Type? resourceType)
        => WithValidationResource(resourceType);

    IFluentTypeValidatorRoot IFluentTypeValidatorRoot.WithCulture(CultureInfo culture)
        => WithCulture(culture);

    #endregion
}


