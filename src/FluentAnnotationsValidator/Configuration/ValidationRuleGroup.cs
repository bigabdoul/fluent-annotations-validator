using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a group of validation rules that apply to a specific member of a model.
/// </summary>
/// <remarks>
/// Encapsulates the rules, the member they apply to, and the model type,
/// providing a cohesive unit for validation and rule management.
/// </remarks>
internal class ValidationRuleGroup(Type objectType, MemberInfo member, List<IValidationRule> rules) : IValidationRuleGroup
{
    /// <inheritdoc/>
    public Type ObjectType { get; } = objectType;

    /// <inheritdoc/>
    public MemberInfo Member { get; } = member;

    /// <inheritdoc/>
    public List<IValidationRule> Rules { get; } = rules;

    /// <inheritdoc/>
    public int RemoveRules(Predicate<MemberInfo> predicate)
        => Rules.RemoveAll(rule => predicate(rule.Member));

    /// <inheritdoc/>
    public int RemoveAttributesOf<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate)
        where TAttribute : ValidationAttribute
    {
        return Rules.RemoveAll(rule =>
            rule.Validator is TAttribute attr &&
            predicate(rule.Member, attr));
    }

    /// <inheritdoc/>
    public int RemoveAttributesOf(MemberInfo member, Type attributeType)
    {
        if (!member.AreSameMembers(Member)) return 0;

        return Rules.RemoveAll(rule =>
            rule.Validator is { } attr &&
            attr.GetType() == attributeType);
    }
}