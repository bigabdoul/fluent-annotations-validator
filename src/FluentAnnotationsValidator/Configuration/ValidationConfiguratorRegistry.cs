namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Registry that maps DTO types to their validation configurator metadata.
/// Used for implicit rule synthesis, culture/resource lookup, and diagnostics.
/// </summary>
public sealed class ValidationConfiguratorRegistry
{
    private readonly Dictionary<Type, ValidationTypeConfiguratorBase> _configurators = [];

    /// <summary>
    /// Gets all registered configurators.
    /// </summary>
    public IReadOnlyDictionary<Type, ValidationTypeConfiguratorBase> Configurators => _configurators;

    /// <summary>
    /// Registers a configurator for a specific DTO type.
    /// </summary>
    public void Register(Type dtoType, ValidationTypeConfiguratorBase configurator)
    {
        _configurators[dtoType] = configurator;
    }

    /// <summary>
    /// Attempts to retrieve a configurator for a given DTO type.
    /// </summary>
    public bool TryGet(Type dtoType, out ValidationTypeConfiguratorBase? configurator)
    {
        return _configurators.TryGetValue(dtoType, out configurator);
    }
}
