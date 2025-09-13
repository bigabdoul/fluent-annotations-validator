using FluentAnnotationsValidator.Abstractions;
using System.Collections.Concurrent;
using System.Globalization;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Registry that maps DTO types to their validation configurator metadata.
/// Used for implicit rule synthesis, culture/resource lookup, and diagnostics.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GlobalRegistry"/> class.
/// </remarks>
public sealed class GlobalRegistry() : IGlobalRegistry
{
    private readonly ConcurrentDictionary<Type, FluentTypeValidatorBase> _fluentTypeConfigurators = [];

    #region Default Instance

    private static readonly Lazy<GlobalRegistry> _lazyDefault =
        new(() => new GlobalRegistry(), isThreadSafe: true);
    
    /// <summary>
    /// Gets the default instance of the <see cref="GlobalRegistry"/> class.
    /// </summary>
    public static GlobalRegistry Default => _lazyDefault.Value;

    #endregion

    #region properties

    /// <summary>
    /// Optional common resource type used for localization.
    /// </summary>
    public Type? SharedResourceType { get; set; }

    /// <summary>
    /// Optional culture to use for error messages and formatting.
    /// </summary>
    public CultureInfo? SharedCulture { get; set; }

    /// <summary>
    /// When true, uses conventional resource key naming (e.g. MemberName_Attribute).
    /// </summary>
    public bool UseConventionalKeys { get; set; } = true;

    /// <summary>
    /// Gets or sets the delegate to retrieve the conventional key aspect.
    /// </summary>
    public ConventionalKeyDelegate? ConventionalKeyGetter { get; set; }

    /// <summary>
    /// Determines whether fluent configurations are checked for consistency.
    /// </summary>
    public bool ConfigurationEnforcementDisabled { get; set; }

    /// <summary>
    /// Determines whether property accessor cache is disabled.
    /// </summary>
    public bool DisableAccessorCache { get; set; }

    #endregion

    /// <summary>
    /// Registers a configurator for a specific DTO type.
    /// </summary>
    public void Register(Type dtoType, FluentTypeValidatorBase configurator)
    {
        _fluentTypeConfigurators[dtoType] = configurator;
    }
}
