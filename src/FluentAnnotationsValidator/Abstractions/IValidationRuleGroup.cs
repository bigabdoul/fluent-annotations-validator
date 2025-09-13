using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines a contract for a group of validation rules that apply to a specific member of a model.
/// </summary>
public interface IValidationRuleGroup
{
    /// <summary>
    /// Gets the type of the model that this rule group applies to.
    /// </summary>
    Type ObjectType { get; }

    /// <summary>
    /// Gets the member (property or field) that this rule group validates.
    /// </summary>
    public MemberInfo Member { get; }

    /// <summary>
    /// Gets the list of individual validation rules within this group.
    /// </summary>
    List<IValidationRule> Rules { get; }

    /// <summary>
    /// Removes all rules from this group that match the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each rule for a condition.</param>
    /// <returns>The number of rules removed from the group.</returns>
    int RemoveRules(Predicate<MemberInfo> predicate);

    /// <summary>
    /// Removes all validation rules of the specified attribute type that satisfy a given predicate.
    /// </summary>
    /// <typeparam name="TAttribute">The type of <see cref="ValidationAttribute"/> to remove.</typeparam>
    /// <param name="predicate">
    /// A function that evaluates each rule's <see cref="MemberInfo"/> and attribute instance to determine whether it should be removed.
    /// </param>
    /// <returns>The number of rules removed from the group.</returns>
    /// <remarks>
    /// This method enables fine-grained removal of rules based on both member identity and attribute configuration.
    /// </remarks>
    int RemoveAttributesOf<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate)
        where TAttribute : ValidationAttribute;

    /// <summary>
    /// Removes all validation rules of the specified attribute type from the given member.
    /// </summary>
    /// <param name="member">The member whose rules should be evaluated for removal.</param>
    /// <param name="attributeType">The exact <see cref="Type"/> of the validation attribute to remove.</param>
    /// <returns>The number of rules removed from the group.</returns>
    /// <remarks>
    /// This method performs strict type matching and member comparison to ensure only targeted rules are removed.
    /// </remarks>
    int RemoveAttributesOf(MemberInfo member, Type attributeType);
}
