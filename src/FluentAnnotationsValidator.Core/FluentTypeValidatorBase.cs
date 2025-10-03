using System.Globalization;

namespace FluentAnnotationsValidator.Core;

/// <summary>
/// Provides a base class for configuring type-level validation metadata.
/// Enables fluent rule synthesis, localized message resolution, and runtime discoverability.
/// </summary>
public abstract class FluentTypeValidatorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentTypeValidatorBase"/> class for a specific model type.
    /// </summary>
    /// <param name="objectType">The <see cref="Type"/> of the model being configured (e.g., <c>typeof(LoginDto)</c>).</param>
    /// <remarks>
    /// Upon instantiation, this configurator is automatically registered with the <see cref="GlobalRegistry"/>,
    /// making it discoverable by the validation engine. This allows fluent-defined rules and localized messages
    /// to be applied dynamically at runtime for the specified <paramref name="objectType"/>.
    /// </remarks>
    protected FluentTypeValidatorBase(Type objectType)
    {
        ObjectType = objectType;
        GlobalRegistry.Default.Register(objectType, this);
    }

    /// <summary>
    /// Gets the model type associated with this configurator.
    /// </summary>
    /// <example><c>typeof(LoginDto)</c></example>
    public Type ObjectType { get; }

    /// <summary>
    /// Gets or sets the culture used for formatting and localized message resolution.
    /// </summary>
    /// <remarks>
    /// If set, this culture will influence how error messages are formatted and resolved from resources.
    /// </remarks>
    public CultureInfo? Culture { get; protected set; }

    /// <summary>
    /// Gets or sets the resource type used for resolving localized error message keys.
    /// </summary>
    /// <remarks>
    /// This type should contain public static string properties or resource keys used by validation attributes.
    /// </remarks>
    public Type? ValidationResourceType { get; protected set; }
}