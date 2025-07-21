using System.Diagnostics.CodeAnalysis;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a configurable container for conditional validation rules applied to specific model properties.
/// </summary>
public class ValidationBehaviorOptions
{
    /// <summary>
    /// Internal dictionary mapping a combination of model type and property name
    /// to a corresponding <see cref="ConditionalValidationRule"/>.
    /// </summary>
    private readonly Dictionary<(Type modelType, string propertyName), ConditionalValidationRule> PropertyConditions = [];

    /// <summary>
    /// Associates a <see cref="ConditionalValidationRule"/> with the specified model type and property name.
    /// If a rule already exists for the given key, it will be replaced.
    /// </summary>
    /// <param name="modelType">The type of the model.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="rule">The conditional validation rule to associate.</param>
    public virtual void Set(Type modelType, string propertyName, ConditionalValidationRule rule)
    {
        PropertyConditions[(modelType, propertyName)] = rule;
    }

    /// <summary>
    /// Retrieves the <see cref="ConditionalValidationRule"/> associated with the specified model type and property name.
    /// </summary>
    /// <param name="modelType">The type of the model.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The associated <see cref="ConditionalValidationRule"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if no rule is found for the given type and property name.
    /// </exception>
    public virtual ConditionalValidationRule Get(Type modelType, string propertyName)
    {
        if (PropertyConditions.TryGetValue((modelType, propertyName), out var rule))
            return rule;

        throw new KeyNotFoundException($"No key found matching the specified type and property name.");
    }

    /// <summary>
    /// Attempts to retrieve a <see cref="ConditionalValidationRule"/> associated with the specified model type and property name.
    /// </summary>
    /// <param name="modelType">The type of the model.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="rule">
    /// When this method returns, contains the associated rule if found and non-null; otherwise, <c>null</c>.
    /// </param>
    /// <returns><see langword="true"/> if a non-null rule was found; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGet(Type modelType, string propertyName, [NotNullWhen(true)] out ConditionalValidationRule? rule)
    {
        return PropertyConditions.TryGetValue((modelType, propertyName), out rule) && rule != null;
    }

    /// <summary>
    /// Determines whether a rule exists for the specified model type and property name.
    /// </summary>
    /// <param name="modelType">The type of the model.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns><see langword="true"/> if a rule exists; otherwise, <see langword="false"/>.</returns>
    public virtual bool ContainsKey(Type modelType, string propertyName)
        => PropertyConditions.ContainsKey((modelType, propertyName));
}
