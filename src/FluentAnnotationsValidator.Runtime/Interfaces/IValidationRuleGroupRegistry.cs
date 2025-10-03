using FluentAnnotationsValidator.Core.Interfaces;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Runtime.Interfaces;

/// <summary>
/// Represents a registry for managing grouped validation rules associated with object members and types.
/// </summary>
public interface IValidationRuleGroupRegistry : IRuleRegistry
{
    /// <summary>
    /// Adds a validation rule for the specified member.
    /// </summary>
    /// <param name="member">The member to associate the rule with.</param>
    /// <param name="rule">The validation rule to add.</param>
    void AddRule(MemberInfo member, IValidationRule rule);

    /// <summary>
    /// Adds a validation rule for the specified member of a given object type.
    /// </summary>
    /// <param name="objectType">The type that declares the member.</param>
    /// <param name="member">The member to associate the rule with.</param>
    /// <param name="rule">The validation rule to add.</param>
    void AddRule(Type objectType, MemberInfo member, IValidationRule rule);

    /// <summary>
    /// Adds a list of validation rule groups for the specified type.
    /// </summary>
    /// <param name="type">The type to associate the rule groups with.</param>
    /// <param name="group">The list of rule groups to add.</param>
    void AddRules(Type type, IList<IValidationRuleGroup> group);

    /// <summary>
    /// Clears all registered validation rules.
    /// </summary>
    void Clear();

    /// <summary>
    /// Determines whether the registry contains any rule group matching the specified expression and optional filter.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="expression">The member expression to check.</param>
    /// <param name="filter">An optional filter to apply to rule groups.</param>
    /// <returns><c>true</c> if a matching rule group exists; otherwise, <c>false</c>.</returns>
    bool Contains<T>(Expression<Func<T, string?>> expression, Func<IValidationRuleGroup, bool>? filter = null);

    /// <summary>
    /// Determines whether any rule for the specified member matches the given filter.
    /// </summary>
    /// <param name="type">The type that declares the member.</param>
    /// <param name="member">The member to check.</param>
    /// <param name="filter">The filter to apply to rules.</param>
    /// <returns><c>true</c> if any matching rule exists; otherwise, <c>false</c>.</returns>
    bool ContainsAny<T>(Type type, MemberInfo member, Func<IValidationRule, bool> filter);

    /// <summary>
    /// Enumerates all rule groups associated with the specified type.
    /// </summary>
    /// <param name="type">The type to enumerate rules for.</param>
    /// <returns>A list of model types and their associated rule groups.</returns>
    List<(Type ModelType, ValidationRuleGroupList Groups)> EnumerateRules(Type type);

    /// <summary>
    /// Enumerates all rule groups associated with the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type to enumerate rules for.</typeparam>
    /// <returns>A list of model types and their associated rule groups.</returns>
    List<(Type ModelType, ValidationRuleGroupList Rules)> EnumerateRules<T>();

    /// <summary>
    /// Finds rule groups matching the specified expression and optional filter.
    /// </summary>
    /// <param name="expression">The member expression to search for.</param>
    /// <param name="filter">An optional filter to apply to rule groups.</param>
    /// <returns>A list of matching rule groups.</returns>
    ValidationRuleGroupList FindRuleGroups(Expression expression, Func<IValidationRuleGroup, bool>? filter = null);

    /// <summary>
    /// Finds rule groups for the specified member of a given object type.
    /// </summary>
    /// <param name="objectType">The type that declares the member.</param>
    /// <param name="member">The member to search for.</param>
    /// <param name="filter">An optional filter to apply to rule groups.</param>
    /// <returns>A list of matching rule groups.</returns>
    ValidationRuleGroupList FindRules(Type objectType, MemberInfo member, Func<IValidationRuleGroup, bool>? filter = null);

    /// <summary>
    /// Gets all rule groups associated with the specified object type.
    /// </summary>
    /// <param name="objectType">The type to retrieve rules for.</param>
    /// <returns>A list of rule groups.</returns>
    ValidationRuleGroupList GetRules(Type objectType);

    /// <summary>
    /// Gets validation rules for the specified member expression, optionally filtered.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="expression">The member expression to retrieve rules for.</param>
    /// <param name="filter">An optional filter to apply to rules.</param>
    /// <returns>A read-only list of matching validation rules.</returns>
    IReadOnlyList<IValidationRule> GetRules<T>(Expression<Func<T, string?>> expression, Func<IValidationRule, bool>? filter = null);

    /// <summary>
    /// Gets rule groups for the specified member expression, optionally filtered.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="expression">The member expression to retrieve rule groups for.</param>
    /// <param name="predicate">An optional filter to apply to rule groups.</param>
    /// <returns>A list of matching rule groups.</returns>
    ValidationRuleGroupList GetRules<T>(Expression<Func<T, string?>> expression, Func<IValidationRuleGroup, bool>? predicate = null);

    /// <summary>
    /// Removes all rules associated with the specified member of a type.
    /// </summary>
    /// <param name="type">The type that declares the member.</param>
    /// <param name="member">The member to remove rules for.</param>
    /// <returns><c>true</c> if any rules were removed; otherwise, <c>false</c>.</returns>
    bool RemoveAll(Type type, MemberInfo member);

    /// <summary>
    /// Removes all rules for the specified member that are decorated with a specific attribute type.
    /// </summary>
    /// <param name="objectType">The type that declares the member.</param>
    /// <param name="member">The member to remove rules for.</param>
    /// <param name="attributeType">The attribute type to match.</param>
    /// <returns>The number of rules removed.</returns>
    int RemoveAll(Type objectType, MemberInfo member, Type attributeType);

    /// <summary>
    /// Removes all rules for members of the specified type that match a predicate.
    /// </summary>
    /// <param name="objectType">The type to search for members.</param>
    /// <param name="predicate">The predicate to match members.</param>
    /// <returns>The number of rules removed.</returns>
    int RemoveAll(Type objectType, Predicate<MemberInfo> predicate);

    /// <summary>
    /// Removes all rules for members decorated with a specific attribute type and matching a predicate.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to match.</typeparam>
    /// <param name="predicate">The predicate to match members and attributes.</param>
    /// <returns>The number of rules removed.</returns>
    int RemoveAll<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate) where TAttribute : ValidationAttribute;

    /// <summary>
    /// Removes all rules for members of the specified type that are decorated with a specific attribute type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to match.</typeparam>
    /// <param name="objectType">The type to search for members.</param>
    /// <returns>The number of rules removed.</returns>
    int RemoveAll<TAttribute>(Type objectType) where TAttribute : ValidationAttribute;

    /// <summary>
    /// Removes all rules for the specified type, optionally filtered by member.
    /// </summary>
    /// <param name="objectType">The type to remove rules for.</param>
    /// <param name="filter">An optional predicate to filter members.</param>
    /// <returns>The number of rules removed.</returns>
    int RemoveAllForType(Type objectType, Predicate<MemberInfo>? filter = null);
    /// <summary>
    /// Gets the internal registry of validation rule groups for a specific member of a given object type.
    /// </summary>
    /// <param name="objectType">The type that declares the member.</param>
    /// <param name="member">The member to retrieve the registry for.</param>
    /// <returns>A concurrent dictionary mapping types to their associated validation rule groups.</returns>
    ConcurrentDictionary<Type, IList<IValidationRuleGroup>> GetRegistryForMember(Type objectType, MemberInfo member);

    /// <summary>
    /// Attempts to retrieve validation rules for the specified member expression, optionally filtered.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="expression">The member expression to retrieve rules for.</param>
    /// <param name="rules">When this method returns, contains the list of matching validation rules, if found.</param>
    /// <param name="filter">An optional filter to apply to the rules.</param>
    /// <returns><c>true</c> if any rules were found; otherwise, <c>false</c>.</returns>
    bool TryGetRules<T>(Expression<Func<T, string?>> expression, out IReadOnlyList<IValidationRule> rules, Func<IValidationRule, bool>? filter = null);

    /// <summary>
    /// Marks the build status of the validator for the specified type.
    /// </summary>
    /// <param name="type">The type whose build status is being set.</param>
    /// <param name="status"><c>true</c> if the validator has been built; otherwise, <c>false</c>.</param>
    void MarkBuilt(Type type, bool status);
}