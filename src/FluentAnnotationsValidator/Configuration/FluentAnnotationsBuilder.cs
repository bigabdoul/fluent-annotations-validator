using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// A builder class for fluently configuring validation services and behaviors.
/// </summary>
/// <remarks>
/// This class encapsulates the <see cref="IServiceCollection"/> and 
/// <see cref="ValidationRuleGroupRegistry"/>, providing a convenient and type-safe 
/// way to define validation rules, pre-validation providers, and other behaviors 
/// using a fluent API. It is the entry point for the fluent configuration system.
/// </remarks>
public sealed class FluentAnnotationsBuilder
{
    #region fields

    private static FluentAnnotationsBuilder? _default;
    private static readonly object _lock = new();

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentAnnotationsBuilder"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to configure.</param>
    /// <param name="ruleRegistry">The <see cref="ValidationRuleGroupRegistry"/> instance that stores all fluent type validators.</param>
    public FluentAnnotationsBuilder(IServiceCollection services, ValidationRuleGroupRegistry ruleRegistry)
    {
        if (_default == null)
        {
            // Set the first initialized instance, and make it the default.
            // There's no implicit initialization when the static Default
            // property is accessed.
            lock (_lock)
            {
                _default ??= this;
            }
        }
        Services = services;
        Registry = ruleRegistry;
    }

    #region properties

    /// <summary>
    /// Gets the default instance of the <see cref="FluentAnnotationsBuilder"/> class.
    /// </summary>
    public static FluentAnnotationsBuilder? Default => _default;

    /// <summary>
    /// Gets the underlying <see cref="IServiceCollection"/> instance being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the <see cref="ValidationBehaviorOptions"/> instance where all
    /// fluent configuration and rules are stored.
    /// </summary>
    [Obsolete("Use the property " + nameof(Registry), true)]
    public ValidationBehaviorOptions Options { get; } = default!;

    /// <summary>
    /// Gets the <see cref="ValidationRuleGroupRegistry"/> instance where all 
    /// fluent configuration validators are stored.
    /// </summary>
    public ValidationRuleGroupRegistry Registry { get; }

    #endregion
}