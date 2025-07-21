namespace FluentAnnotationsValidator.Interfaces;

/// <summary>
/// Defines the contract for configuring conditional validation rules
/// in a fluent and type-safe manner.
/// </summary>
public interface IValidationConfigurator
{
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

