using System.Globalization;

namespace FluentAnnotationsValidator.Core.Interfaces;

/// <summary>
/// Defines a global registry for managing validation configuration, shared resources, and test context.
/// </summary>
public interface IGlobalRegistry
{
    /// <summary>
    /// Gets or sets a value indicating whether enforcement of configuration completeness is disabled.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, missing rules or incomplete configurations will not trigger enforcement errors.
    /// Useful for exploratory testing or partial validation scenarios.
    /// </remarks>
    bool ConfigurationEnforcementDisabled { get; set; }

    /// <summary>
    /// Gets or sets the delegate used to resolve conventional resource keys for validation messages.
    /// </summary>
    /// <remarks>
    /// This delegate enables dynamic key generation for localization, based on property names, types, or rule metadata.
    /// </remarks>
    ConventionalKeyDelegate? ConventionalKeyGetter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether accessor caching is disabled.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, property accessors are resolved dynamically on each invocation.
    /// This may reduce performance but improves traceability and override safety in dynamic scenarios.
    /// </remarks>
    bool DisableAccessorCache { get; set; }

    /// <summary>
    /// Gets or sets the shared culture used for formatting and localization.
    /// </summary>
    /// <remarks>
    /// This culture is applied to all validators unless overridden locally.
    /// </remarks>
    CultureInfo? SharedCulture { get; set; }

    /// <summary>
    /// Gets or sets the shared resource type used for resolving localized validation messages.
    /// </summary>
    /// <remarks>
    /// This type typically contains resource keys for error messages and display names.
    /// </remarks>
    Type? SharedResourceType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether conventional resource keys should be used.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, validators will attempt to resolve message keys using naming conventions.
    /// </remarks>
    bool UseConventionalKeys { get; set; }

    /// <summary>
    /// Registers a validator configuration for the specified DTO type.
    /// </summary>
    /// <param name="dtoType">The DTO type to associate with the validator.</param>
    /// <param name="configurator">The validator configuration to register.</param>
    /// <remarks>
    /// This method enables centralized rule registration and reuse across validation pipelines.
    /// </remarks>
    void Register(Type dtoType, FluentTypeValidatorBase configurator);
}