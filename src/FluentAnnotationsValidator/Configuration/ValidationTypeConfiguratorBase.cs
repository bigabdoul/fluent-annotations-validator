using System.Globalization;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Base configurator holding type-level validation metadata.
/// Enables implicit rule synthesis and message resolution.
/// </summary>
[Obsolete("Use FluentAnnotationsValidator.Configuration.FluentTypeValidatorBase", true)]
public abstract class ValidationTypeConfiguratorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationTypeConfiguratorBase"/> class for a specific target type.
    /// </summary>
    /// <remarks>
    /// This constructor automatically registers the configurator instance with the global
    /// <see cref="GlobalRegistry"/>. This ensures that the configurator is
    /// discoverable by the validation framework, allowing the engine to retrieve and apply
    /// fluent-defined rules for the specified <paramref name="targetType"/> at runtime.
    /// </remarks>
    /// <param name="targetType">The <see cref="Type"/> of the model for which this configurator will define validation rules.</param>
    protected ValidationTypeConfiguratorBase(Type targetType)
    {
        //TargetType = targetType;
        //GlobalValidationRegistry.Default.Register(targetType, this);
        throw new NotSupportedException("This class is obsolete and will be removed in the final release. " +
            $"Use the {typeof(FluentTypeValidatorBase).Name} class.");
    }

    /// <summary>
    /// Type being configured (e.g. typeof(LoginDto)).
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Culture used for formatting and message resolution.
    /// </summary>
    public CultureInfo? Culture { get; protected set; }

    /// <summary>
    /// Resource class for error message keys.
    /// </summary>
    public Type? ValidationResourceType { get; protected set; }
}
