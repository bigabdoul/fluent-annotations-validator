using System.Linq.Expressions;

namespace FluentAnnotationsValidator;

/// <summary>
/// Provides a fluent interface for configuring conditional validation rules on a specific model type.
/// Supports chaining validation logic, metadata overrides, and transitions to other model configurators.
/// </summary>
/// <typeparam name="T">The model type being configured.</typeparam>
public interface IValidationTypeConfigurator<T>
{
    /// <summary>
    /// Adds a conditional validation rule for a given property.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="property">An expression identifying the target property.</param>
    /// <param name="condition">A predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> When<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition);

    /// <summary>
    /// Adds an additional conditional rule for another property. Alias for <see cref="When{TProp}"/>.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="property">An expression identifying the target property.</param>
    /// <param name="condition">A predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> And<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition);

    /// <summary>
    /// Excludes the specified property from validation entirely.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to exclude.</typeparam>
    /// <param name="property">An expression identifying the property to skip.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> Except<TProp>(
        Expression<Func<T, TProp>> property);

    /// <summary>
    /// Forces unconditional validation for the specified property.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to validate.</typeparam>
    /// <param name="property">An expression identifying the property to validate.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> AlwaysValidate<TProp>(Expression<Func<T, TProp>> property);

    /// <summary>
    /// Attaches a custom error message to the current validation condition.
    /// </summary>
    /// <param name="message">The text to display when the condition fails.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> WithMessage(string message);

    /// <summary>
    /// Attaches an explicit error key to the current validation condition.
    /// </summary>
    /// <param name="key">The failure key used by the message resolver or diagnostics.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> WithKey(string key);

    /// <summary>
    /// Specifies a resource key for localized error messages tied to the current condition.
    /// </summary>
    /// <param name="resourceKey">The key used to lookup localized text from a resource provider.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IValidationTypeConfigurator<T> Localized(string resourceKey);

    /// <summary>
    /// Transitions to configuring a different model type.
    /// </summary>
    /// <typeparam name="TNext">The next model type to configure.</typeparam>
    /// <returns>A configurator for the specified model type.</returns>
    IValidationTypeConfigurator<TNext> For<TNext>();

    /// <summary>
    /// Finalizes the configuration by registering all buffered rules into the underlying options system.
    /// </summary>
    void Build();
}
