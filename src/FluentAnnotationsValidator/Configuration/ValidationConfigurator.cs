using FluentAnnotationsValidator.Abstractions;
using System.Globalization;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Provides a fluent API for registering conditional validation rules and behaviors
/// into the application’s service collection.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationConfigurator"/> class
/// with the specified service collection.
/// </remarks>
/// <param name="options">The validation behavior options used during <see cref="Build"/>.</param>
public class ValidationConfigurator(ValidationBehaviorOptions options) : IValidationConfigurator
{
    private readonly List<Action<ValidationBehaviorOptions>> _registrations = [];

    /// <summary>
    /// Begins configuring conditional validation rules for a specific model type.
    /// </summary>
    /// <typeparam name="T">The model type to configure validation rules for.</typeparam>
    /// <returns>A <see cref="ValidationTypeConfigurator{T}"/> to define rules for the specified type.</returns>
    public virtual ValidationTypeConfigurator<T> For<T>() => new(this, options);

    /// <summary>
    /// Gets the validation behavior options.
    /// </summary>
    public ValidationBehaviorOptions Options => options;

    /// <inheritdoc cref="IValidationConfigurator.WithValidationResource{TResource}()"/>
    public ValidationConfigurator WithValidationResource<TResource>() => WithValidationResource(typeof(TResource));

    /// <inheritdoc cref="IValidationConfigurator.WithValidationResource(Type?)"/>
    public ValidationConfigurator WithValidationResource(Type? resourceType)
    {
        options.CommonResourceType = resourceType;
        AssignCultureTo(resourceType);
        return this;
    }

    /// <inheritdoc cref="IValidationConfigurator.WithCulture(CultureInfo)"/>
    public ValidationConfigurator WithCulture(CultureInfo culture)
    {
        options.CommonCulture = culture;
        return this;
    }

    /// <summary>
    /// Begins configuring conditional validation rules for a specific model type.
    /// (Explicit interface implementation version.)
    /// </summary>
    /// <typeparam name="T">The model type to configure validation rules for.</typeparam>
    /// <returns>An <see cref="IValidationTypeConfigurator{T}"/> to define rules for the specified type.</returns>
    IValidationTypeConfigurator<T> IValidationConfigurator.For<T>() => For<T>();

    /// <summary>
    /// Registers a configuration delegate that modifies the <see cref="ValidationBehaviorOptions"/>.
    /// </summary>
    /// <param name="config">The configuration action to apply.</param>
    public virtual void Register(Action<ValidationBehaviorOptions> config)
        => _registrations.Add(config);

    /// <summary>
    /// Applies all registered validation configurations to the service collection.
    /// Should be called after all rules have been configured.
    /// </summary>
    public virtual void Build()
    {
        foreach (var action in _registrations)
            action(options);
    }

    private void AssignCultureTo(Type? type)
    {
        if (type != null)
        {
            var prop = type.GetProperty("Culture", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            prop?.SetValue(null, options.CommonCulture);
        }
    }

    #region IValidationConfigurator

    IValidationConfigurator IValidationConfigurator.WithValidationResource<TResource>()
        => WithValidationResource<TResource>();

    IValidationConfigurator IValidationConfigurator.WithValidationResource(Type? resourceType)
        => WithValidationResource(resourceType);

    IValidationConfigurator IValidationConfigurator.WithCulture(CultureInfo culture)
        => WithCulture(culture);

    #endregion
}


