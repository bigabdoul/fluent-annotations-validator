using System.Reflection;

namespace FluentAnnotationsValidator.Core.Interfaces;

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
    /// <returns>A <see cref="List{IAbstractValidationRule}"/> containing the rules for the specified type.</returns>
    List<IValidationRule> GetRulesForType(Type type);

    /// <summary>
    /// Retrieves a list of validation rules that have been configured for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for which to retrieve configured rules.</typeparam>
    /// <returns>A list of <see cref="IValidationRule{T}"/> containing the rules for the specified type.</returns>
    List<IValidationRule<T>> GetRulesForType<T>();

    /// <summary>
    /// Retrieves all rules for the specified type, and groups 
    /// them on the <see cref="IValidationRule.Member"/> property.
    /// </summary>
    /// <param name="forType">The type for which to retrieve rules.</param>
    /// <returns>
    /// A collection of <see cref="IValidationRule"/> objects 
    /// grouped on the property <see cref="IValidationRule.Member"/>.
    /// </returns>
    IEnumerable<IGrouping<MemberInfo, IValidationRule>> GetRulesByMember(Type forType);
}
