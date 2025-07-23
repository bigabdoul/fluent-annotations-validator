using System.Globalization;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Base configurator holding type-level validation metadata.
/// Enables implicit rule synthesis and message resolution.
/// </summary>
public abstract class ValidationTypeConfiguratorBase
{
    protected internal Dictionary<string, ConditionalValidationRule> Rules { get; } = [];

    protected ValidationTypeConfiguratorBase(Type targetType)
    {
        TargetType = targetType;
        ValidationConfiguratorStore.Instance.Register(targetType, this);
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
