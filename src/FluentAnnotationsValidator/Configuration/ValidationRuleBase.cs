using System.Globalization;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Contains all the properties and methods that are common to rule based class.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationRuleBase"/> class.
/// </remarks>
/// <param name="message">The validation error message.</param>
/// <param name="key">The failure key used by the message resolver or diagnostics.</param>
/// <param name="resourceKey">The resource manager's key for retrieving a localized error message.</param>
/// <param name="resourceType">The resource manager's type for localized error message.</param>
/// <param name="culture">The culture to use in the resource manager.</param>
/// <param name="fallbackMessage">The fallback error message if resolution fails.</param>
/// <param name="useConventionalKeys">Explicitly disables "Property_Attribute" fallback lookup.</param>
public abstract class ValidationRuleBase(
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool? useConventionalKeys = true
    )
{
    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? Message { get; set; } = message;

    /// <summary>
    /// Gets or sets the overridden property name.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the failure key used by the message resolver or diagnostics.
    /// </summary>
    public string? Key { get; set; } = key;

    /// <summary>
    /// Gets or sets the resource manager's key for retrieving a localized error message.
    /// </summary>
    public string? ResourceKey { get; set; } = resourceKey;

    /// <summary>
    /// Gets or sets the resource manager's type for localized error message.
    /// </summary>
    public Type? ResourceType { get; set; } = resourceType;

    /// <summary>
    /// Gets or sets the culture to use in the resource manager.
    /// </summary>
    public CultureInfo? Culture { get; set; } = culture;

    /// <summary>
    /// Gets or sets a fallback error message if resolution fails.
    /// </summary>
    public string? FallbackMessage { get; set; } = fallbackMessage;

    /// <summary>
    /// A delegate to perform custom instance configuration before validation occurs.
    /// </summary>
    public PreValidationValueProviderDelegate? ConfigureBeforeValidation { get; set; }

    /// <summary>
    /// Explicitly disables "Property_Attribute" fallback lookup.
    /// </summary>
    public bool? UseConventionalKeys { get; set; } = useConventionalKeys;
}