using FluentAnnotationsValidator.Core.Extensions;
using FluentAnnotationsValidator.Core.Interfaces;
using FluentAnnotationsValidator.Runtime.Interfaces;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Runtime;

/// <summary>
/// Central configuration point for FluentAnnotationsValidator.
/// Stores validation rules mapped to each property or field.
/// Supports multiple validation attributes per type.
/// </summary>
public class ValidationRuleGroupRegistry : IValidationRuleGroupRegistry
{
    private readonly ConcurrentDictionary<Type, ValidationRuleGroupList> _ruleRegistry = new();
    private readonly ConcurrentDictionary<Type, bool> _isBuiltByType = new();

    #region Default

    private static readonly Lazy<ValidationRuleGroupRegistry> _lazyDefault =
        new(() => new ValidationRuleGroupRegistry(), isThreadSafe: true);

    /// <summary>
    /// Gets the default instance of the <see cref="ValidationRuleGroupRegistry"/> class.
    /// </summary>
    public static ValidationRuleGroupRegistry Default => _lazyDefault.Value;

    #endregion

    /// <summary>
    /// Registers a validation rule for the specified type.
    /// Multiple rules can be associated with a single type.
    /// </summary>
    /// <param name="type">The target property or field.</param>
    /// <param name="group">The validation rule to apply.</param>
    public virtual void AddRules(Type type, IList<IValidationRuleGroup> group)
    {
        // Use GetOrAdd to retrieve the list for the type, or create a new one if it doesn't exist.
        // This is more efficient than AddOrUpdate for this specific pattern.
        var existing = _ruleRegistry.GetOrAdd(type, new ValidationRuleGroupList(type));
        lock (existing)
        {
            existing.Merge(group);
        }
        MarkBuilt(type, true);
    }

    /// <summary>
    /// Registers a validation rule for the specified member.
    /// </summary>
    /// <param name="member">The target property or field.</param>
    /// <param name="rule">The validation rule to apply.</param>
    /// <exception cref="InvalidOperationException">
    /// Both <see cref="MemberInfo.ReflectedType"/> and <see cref="MemberInfo.DeclaringType"/> are <see langword="null"/>.
    /// </exception>
    public virtual void AddRule(MemberInfo member, IValidationRule rule)
        => AddRule(GetTypeFromMember(member), member, rule);

    /// <summary>
    /// Registers a validation rule for the specified type and member.
    /// Multiple rules can be associated with a single member.
    /// </summary>
    /// <param name="objectType">The target type.</param>
    /// <param name="member">The target property or field.</param>
    /// <param name="rule">The validation rule to apply.</param>
    public virtual void AddRule(Type objectType, MemberInfo member, IValidationRule rule)
    {
        // Use GetOrAdd to retrieve the list for the member, or create a new one if it
        // doesn't exist. This is more efficient than AddOrUpdate for this specific pattern.
        var group = _ruleRegistry.GetOrAdd(objectType, new ValidationRuleGroupList(objectType));

        // Lock on the specific list instance to ensure thread-safe addition.
        // This provides fine-grained locking, preventing contention on the entire dictionary.
        lock (group)
        {
            group.Merge(new ValidationRuleGroup(objectType, member, [rule]));
        }
    }


    /// <summary>
    /// Retrieves all rules defined for the specified type.
    /// </summary>
    /// <param name="expression">The property or field expression to inspect.</param>
    /// <param name="filter">Optional filter applied to rules.</param>
    /// <returns>A read-only list of rules, or an empty list if none are registered.</returns>
    public virtual IReadOnlyList<IValidationRule> GetRules<T>(Expression<Func<T, string?>> expression,
    Func<IValidationRule, bool>? filter = null)
    {
        var member = expression.GetMemberInfo();
        if (!_ruleRegistry.TryGetValue(typeof(T), out var groups))
        {
            return [];
        }
        var rules = groups.SelectMany(g => g.Rules.Where(r => member.AreSameMembers(r.Member)));
        return filter != null ? [.. rules.Where(filter)] : [.. rules];
    }

    /// <summary>
    /// Retrieves all rules defined for the specified type.
    /// </summary>
    /// <param name="objectType">The property or field to inspect.</param>
    /// <returns>A read-only list of rules, or an empty list if none are registered.</returns>
    public virtual ValidationRuleGroupList GetRules(Type objectType)
        => _ruleRegistry.TryGetValue(objectType, out var rules)
            ? rules
            : new ValidationRuleGroupList(objectType);

    /// <summary>
    /// Retrieves rules associated with the property referenced by the given lambda expression.
    /// Throws if no rules match.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="predicate">Optional filter applied to rules.</param>
    /// <returns>A read-only list of matching rules.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no rule matches the expression.</exception>
    public virtual ValidationRuleGroupList GetRules<T>(Expression<Func<T, string?>> expression,
    Func<IValidationRuleGroup, bool>? predicate = null)
    {
        var member = expression.GetMemberInfo();
        var type = GetTypeFromMember(member);
        var rules = FindRules(type, member, predicate);
        return rules.Count > 0 ? rules : throw new InvalidOperationException("No rule matched the specified expression.");
    }

    /// <summary>
    /// Attempts to retrieve rules for the given expression.
    /// Returns false if no match is found or an error occurs.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="rules">The resulting rule list (empty if none).</param>
    /// <param name="filter">Optional filter applied to rules.</param>
    /// <returns>True if matching rules were found, false otherwise.</returns>
    public virtual bool TryGetRules<T>(Expression<Func<T, string?>> expression,
    out IReadOnlyList<IValidationRule> rules,
    Func<IValidationRule, bool>? filter = null)
    {
        var member = expression.GetMemberInfo();
        var type = GetTypeFromMember(member);

        if (!_ruleRegistry.TryGetValue(type, out var groups))
        {
            rules = [];
            return false;
        }

        var foundRules = groups.SelectMany(g =>
            g.Rules.Where(r => member.AreSameMembers(r.Member) && (filter is null || filter(r)))
        );

        rules = [.. foundRules];
        return rules.Count > 0;
    }

    /// <summary>
    /// Determines if any rule exists for the given expression.
    /// </summary>
    /// <typeparam name="T">Declaring type of the type.</typeparam>
    /// <param name="expression">Expression referencing the type.</param>
    /// <param name="filter">Optional filter applied to rules.</param>
    /// <returns>True if at least one matching rule exists, false otherwise.</returns>
    public virtual bool Contains<T>(Expression<Func<T, string?>> expression,
    Func<IValidationRuleGroup, bool>? filter = null)
    {
        var member = expression.GetMemberInfo();
        var type = GetTypeFromMember(member);
        var groups = FindRules(type, member, filter);
        return groups.Count > 0;
    }

    /// <summary>
    /// Determines if any rule exists for the given type using a predicate to test each one,
    /// and returns true as soon as the first rule satisfies the predicate; otherwise false.
    /// </summary>
    /// <typeparam name="T">The declaring type of the type.</typeparam>
    /// <param name="type">The type information to look up.</param>
    /// <param name="member">The member to match.</param>
    /// <param name="filter">A function that tests each rule.</param>
    /// <returns>
    /// <see langword="true"/> as soon as the first rule satisfies the
    /// <paramref name="filter"/>; otherwise <see langword="false"/>.
    /// </returns>
    public virtual bool ContainsAny<T>(Type type, MemberInfo member, Func<IValidationRule, bool> filter)
    {
        var groups = GetRules(type);
        foreach (var group in groups)
        {
            foreach (var rule in group.Rules)
            {
                if (!member.AreSameMembers(rule.Member)) continue;
                if (filter(rule)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Finds all rules for a given type access expression.
    /// </summary>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="filter">Optional rule filter.</param>
    /// <returns>A read-only list of matching rules.</returns>
    public ValidationRuleGroupList FindRuleGroups(Expression expression, Func<IValidationRuleGroup, bool>? filter = null)
    {
        var member = expression.GetMemberInfo();
        Type type = GetTypeFromMember(member);
        var rules = FindRules(type, member, filter);
        return rules;
    }

    /// <summary>
    /// Finds all rules for a given <see cref="Type"/>, <see cref="MemberInfo"/>, and optional filter.
    /// </summary>
    /// <param name="objectType">The type to look up.</param>
    /// <param name="member">The member for which to retrieve rules.</param>
    /// <param name="filter">Optional rule filter.</param>
    /// <returns>A read-only list of matching rules.</returns>
    public virtual ValidationRuleGroupList FindRules(Type objectType, MemberInfo member,
    Func<IValidationRuleGroup, bool>? filter = null)
    {
        if (!_ruleRegistry.TryGetValue(objectType, out var rules))
        {
            return new ValidationRuleGroupList(objectType);
        }

        var vrg = new ValidationRuleGroupList(objectType);

        foreach (var rule in rules.Where(r => member.AreSameMembers(r.Member)))
        {
            if (filter == null || filter(rule))
            {
                vrg.Add(rule);
            }
        }

        return vrg;
    }

    /// <summary>
    /// Returns all rules associated with members declared in the specified type <typeparamref name="T"/>.
    /// This is useful for introspection and diagnostics.
    /// </summary>
    /// <typeparam name="T">The declaring type to filter by.</typeparam>
    /// <returns>A list of tuples (Type, Rule List) for each matched type.</returns>
    public virtual List<(Type ModelType, ValidationRuleGroupList Rules)> EnumerateRules<T>()
        => EnumerateRules(typeof(T));

    /// <summary>
    /// Enumerates all validation rules that apply to the specified type, organizing them by type.
    /// The result is cached for subsequent lookups to improve performance.
    /// </summary>
    /// <param name="type">The type to enumerate rules for.</param>
    /// <returns>
    /// A list of tuples, where each tuple contains a <see cref="Type"/> and a list of
    /// <see cref="IValidationRuleGroup"/> that apply to that type for the given type.
    /// </returns>
    public virtual List<(Type ModelType, ValidationRuleGroupList Groups)> EnumerateRules(Type type)
    {
        var result = new List<(Type, ValidationRuleGroupList)>();
        if (_ruleRegistry.TryGetValue(type, out var groups))
        {
            result.Add((type, groups));
        }
        return result;
    }

    /// <inheritdoc />
    public List<IValidationRule<T>> GetRulesForType<T>()
        => [.. GetRulesForType(typeof(T)).OfType<IValidationRule<T>>()];

    /// <inheritdoc />
    public virtual List<IValidationRule> GetRulesForType(Type type)
    {
        if (!_isBuiltByType.TryGetValue(type, out var built) || !built)
            throw new FluentValidationException($"Validation rules for {type.Name} were not finalized. " +
                $"Did you forget to call FluentTypeValidator<{type.Name}>.Build()?");

        return !_ruleRegistry.TryGetValue(type, out var groups)
            ? []
            : [.. groups.SelectMany(g => g.Rules)];
    }

    /// <inheritdoc/>
    public IEnumerable<IGrouping<MemberInfo, IValidationRule>> GetRulesByMember(Type forType)
    {
        var rules = GetRulesForType(forType);
        return rules
            .Where(rule => (rule.Member as Type) == forType || true == rule.Member.ReflectedType?.IsAssignableFrom(forType))
            .GroupBy(r => r.Member);
    }

    /// <summary>
    /// Removes all rules registered for the specified type and member.
    /// </summary>
    /// <param name="type">The type whose rules should be removed.</param>
    /// <param name="member">The member whose rules to remove.</param>
    /// <returns>True if any rules were removed, false otherwise.</returns>
    public bool RemoveAll(Type type, MemberInfo member)
        => RemoveAllForType(type, member.AreSameMembers) > 0;

    /// <summary>
    /// Removes all rules matching the specified predicate from all members.
    /// </summary>
    /// <param name="objectType">The type for which to remove rules according to the specified predicate.</param>
    /// <param name="predicate">
    /// A function that takes a Type and returns true if the rule should be removed.
    /// </param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll(Type objectType, Predicate<MemberInfo> predicate)
    {
        return !_ruleRegistry.TryGetValue(objectType, out var groups)
            ? 0
            : groups.RemoveRulesForMember(predicate);
    }

    /// <summary>
    /// Removes all rules associated with the specified attribute type for a given type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of validation attribute to filter by.</typeparam>
    /// <param name="objectType">The type whose rules should be removed.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll<TAttribute>(Type objectType) where TAttribute : ValidationAttribute
        => RemoveAll<TAttribute>((type, _) => objectType.AreSameMembers(type));

    /// <summary>
    /// Removes all rules associated with the specified attribute type for a given type.
    /// </summary>
    /// <param name="objectType">The type whose rules should be removed.</param>
    /// <param name="member">The member for which to remove attributes of the specified type.</param>
    /// <param name="attributeType">The type of validation attribute to filter by.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll(Type objectType, MemberInfo member, Type attributeType)
    {
        var removedCount = 0;
        if (!_ruleRegistry.TryGetValue(objectType, out var groups))
        {
            return 0;
        }
        removedCount += groups.RemoveAttributesOf(member, attributeType);
        return removedCount;
    }

    /// <summary>
    /// Removes all rules matching the specified predicate for a given attribute type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of validation attribute to filter by.</typeparam>
    /// <param name="predicate">A function that takes a <see cref="MemberInfo"/> and an attribute instance.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate) where TAttribute : ValidationAttribute
    {
        var removedCount = 0;
        foreach (var type in _ruleRegistry.Keys)
        {
            if (_ruleRegistry.TryGetValue(type, out var rules))
                removedCount += rules.RemoveAttributesOf(predicate);
        }
        return removedCount;
    }

    /// <summary>
    /// Removes all rules associated with the specified type, and member determined by the predicate.
    /// </summary>
    /// <param name="objectType">The type whose members' rules should be removed.</param>
    /// <param name="filter">
    /// An optional function that determines whether a key should be
    /// removed from the registry when a compatible type is matched.
    /// </param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAllForType(Type objectType, Predicate<MemberInfo>? filter = null)
    {
        return !_ruleRegistry.TryGetValue(objectType, out var groups)
            ? 0
            : groups.RemoveAll(g => filter is null || filter(g.Member));
    }

    /// <summary>
    /// Clears the rule registry.
    /// </summary>
    public void Clear() => _ruleRegistry.Clear();

    /// <summary>
    /// Gets a deep copy of the internal rule registry for the specified type.
    /// </summary>
    /// <param name="objectType">The <see cref="Type"/> of the type to retrieve.</param>
    /// <param name="member">The member to retrieve.</param>
    /// <remarks>
    /// This method provides a snapshot of the current state of the rule registry,
    /// preventing external code from directly modifying the validator's internal configuration.
    /// </remarks>
    /// <returns>A new <see cref="ConcurrentDictionary{TKey, TValue}"/> containing copies
    /// of the type-rule mappings.</returns>
    public ConcurrentDictionary<Type, IList<IValidationRuleGroup>> GetRegistryForMember(Type objectType, MemberInfo member)
    {
        if (!_ruleRegistry.TryGetValue(objectType, out var groups))
        {
            return [];
        }

        var filteredGroups = groups.Where(g => member.AreSameMembers(g.Member));
        var result = new ConcurrentDictionary<Type, IList<IValidationRuleGroup>>();
        if (filteredGroups.Any())
        {
            var newList = new ValidationRuleGroupList(objectType);
            newList.AddRange([.. filteredGroups]);
            result.TryAdd(objectType, newList);
        }

        return result;
    }

    /// <inheritdoc/>
    public void MarkBuilt(Type type, bool status)
    {
        _isBuiltByType[type] = status;
    }

    private static Type GetTypeFromMember(MemberInfo member) => member.ReflectedType ?? member.DeclaringType ??
        throw new InvalidOperationException($"Could not determine the reflected or declaring type of the member.");
}