using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// A builder class for fluently configuring validation services and behaviors.
/// </summary>
/// <remarks>
/// This class encapsulates the <see cref="IServiceCollection"/> and <see cref="ValidationBehaviorOptions"/>,
/// providing a convenient and type-safe way to define validation rules, pre-validation
/// providers, and other behaviors using a fluent API. It is the entry point for
/// the fluent configuration system.
/// </remarks>
/// <param name="services">The <see cref="IServiceCollection"/> instance to configure.</param>
/// <param name="options">The <see cref="ValidationBehaviorOptions"/> instance to store the configuration.</param>
public sealed class FluentAnnotationsBuilder(IServiceCollection services, ValidationBehaviorOptions options)
{
    /// <summary>
    /// Gets the underlying <see cref="IServiceCollection"/> instance being configured.
    /// </summary>
    public IServiceCollection Services { get; init; } = services;

    /// <summary>
    /// Gets the <see cref="ValidationBehaviorOptions"/> instance where all
    /// fluent configuration and rules are stored.
    /// </summary>
    public ValidationBehaviorOptions Options { get; init; } = options;
}