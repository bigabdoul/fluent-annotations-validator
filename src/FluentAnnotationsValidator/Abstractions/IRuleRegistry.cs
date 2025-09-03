using FluentAnnotationsValidator.Configuration;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines a contract for a service that can retrieve configured validation rules
/// for a given type.
/// </summary>
public interface IRuleRegistry
{
    /// <summary>
    /// Retrieves a list of validation rules that have been configured for the specified type.
    /// </summary>
    /// <param name="type">The type for which to retrieve the validation rules.</param>
    /// <returns>A <see cref="List{ConditionalValidationRule}"/> containing the rules for the specified type.</returns>
    List<ConditionalValidationRule> GetRulesForType(Type type);
}
