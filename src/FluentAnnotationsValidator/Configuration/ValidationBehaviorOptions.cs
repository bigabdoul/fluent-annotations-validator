using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using static FluentAnnotationsValidator.Internals.Reflection.TypeUtils;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Central configuration point for FluentAnnotationsValidator.
/// Stores conditional validation rules mapped to each property or field.
/// Supports multiple validation attributes per member.
/// </summary>
public class ValidationBehaviorOptions : IRuleRegistry
{
    private static readonly InvalidOperationException NoMatchingRule =
        new("Found no rule matching the specified expression.");

    private readonly ConcurrentDictionary<MemberInfo, List<ConditionalValidationRule>> _ruleRegistry = new();
    private static readonly ConcurrentDictionary<Type, List<(MemberInfo Member, List<ConditionalValidationRule> Rules)>> _cachedRules = new();

    #region properties

    /// <summary>
    /// Optional common resource type used for localization.
    /// </summary>
    public Type? SharedResourceType { get; set; }

    /// <summary>
    /// Optional culture to use for error messages and formatting.
    /// </summary>
    public CultureInfo? SharedCulture { get; set; }

    /// <summary>
    /// When true, uses conventional resource key naming (e.g. MemberName_Attribute).
    /// </summary>
    public bool UseConventionalKeys { get; set; } = true;

    /// <summary>
    /// Gets or sets the delegate to retrieve the conventional key aspect.
    /// </summary>
    public ConventionalKeyDelegate? ConventionalKeyGetter { get; set; }

    /// <summary>
    /// Used when running tests.
    /// </summary>
    public string? CurrentTestName { get; set; }

    /// <summary>
    /// Determines whether fluent configurations are checked for consistency.
    /// </summary>
    public bool ConfigurationEnforcementDisabled { get; set; }

    #endregion

    /// <summary>
    /// Clears the internal cache of enumerated rules. This should be called if rules are
    /// dynamically added or modified after initial configuration.
    /// </summary>
    public static void ClearCache()
    {
        _cachedRules.Clear();
    }

    /// <summary>
    /// Adds or updates a list of conditional validation rules for a specific member.
    /// </summary>
    /// <remarks>
    /// This method is designed for internal and inherited use within the validation framework.
    /// It uses a thread-safe approach to add or replace the rules associated with a given
    /// <see cref="MemberInfo"/> in the internal rule registry. This ensures that new
    /// rules can be added dynamically without risking concurrency issues.
    /// </remarks>
    /// <param name="member">The <see cref="MemberInfo"/> representing the property or field to which the rules apply.</param>
    /// <param name="rules">The <see cref="List{T}"/> of <see cref="ConditionalValidationRule"/> instances to associate with the member.</param>
    protected internal virtual void AddRules(MemberInfo member, List<ConditionalValidationRule> rules)
    {
        var existingRules = _ruleRegistry.GetOrAdd(member, []);

        // Lock on the specific list instance to ensure thread-safe addition.
        lock (existingRules)
        {
            existingRules.AddRange(rules);
        }
    }

    /// <summary>
    /// Registers a conditional validation rule for the specified member.
    /// Multiple rules can be associated with a single member.
    /// </summary>
    /// <param name="member">The target property or field.</param>
    /// <param name="rule">The conditional validation rule to apply.</param>
    public virtual void AddRule(MemberInfo member, ConditionalValidationRule rule)
    {
        // Use GetOrAdd to retrieve the list for the member, or create a new one if it doesn't exist.
        // This is more efficient than AddOrUpdate for this specific pattern.
        var rules = _ruleRegistry.GetOrAdd(member, []);

        // Lock on the specific list instance to ensure thread-safe addition.
        // This provides fine-grained locking, preventing contention on the entire dictionary.
        lock (rules)
        {
            rules.Add(rule);
        }
    }

    /// <summary>
    /// Retrieves all rules defined for the specified member.
    /// </summary>
    /// <param name="member">The property or field to inspect.</param>
    /// <returns>A read-only list of rules, or an empty list if none are registered.</returns>
    public virtual IReadOnlyList<ConditionalValidationRule> GetRules(MemberInfo member)
        => _ruleRegistry.TryGetValue(member, out var rules)
            ? rules.ToList()
            : [];

    /// <summary>
    /// Retrieves rules associated with the property referenced by the given lambda expression.
    /// Throws if no rules match.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="predicate">Optional filter applied to rules.</param>
    /// <returns>A read-only list of matching rules.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no rule matches the expression.</exception>
    public virtual IReadOnlyList<ConditionalValidationRule> GetRules<T>(
        Expression<Func<T, string?>> expression,
        Func<ConditionalValidationRule, bool>? predicate = null)
        => FindRules(expression, predicate) ?? throw NoMatchingRule;

    /// <summary>
    /// Attempts to retrieve rules for the given expression.
    /// Returns false if no match is found or an error occurs.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="rules">The resulting rule list (empty if none).</param>
    /// <param name="predicate">Optional filter applied to rules.</param>
    /// <returns>True if matching rules were found, false otherwise.</returns>
    public virtual bool TryGetRules<T>(
        Expression<Func<T, string?>> expression,
        out IReadOnlyList<ConditionalValidationRule> rules,
        Func<ConditionalValidationRule, bool>? predicate = null)
    {
        try
        {
            rules = FindRules(expression, predicate);
            return rules?.Count > 0;
        }
        catch
        {
            rules = [];
            return false;
        }
    }

    /// <summary>
    /// Determines if any rule exists for the given expression.
    /// </summary>
    /// <typeparam name="T">Declaring type of the member.</typeparam>
    /// <param name="expression">Expression referencing the member.</param>
    /// <param name="predicate">Optional filter applied to rules.</param>
    /// <returns>True if at least one matching rule exists, false otherwise.</returns>
    public virtual bool Contains<T>(Expression<Func<T, string?>> expression, Func<ConditionalValidationRule, bool>? predicate = null)
    {
        try
        {
            return FindRules(expression, predicate).Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if any rule exists for the given member using a predicate to test each one,
    /// and returns true as soon as the first rule satisfies the predicate; otherwise false.
    /// </summary>
    /// <typeparam name="T">The declaring type of the member.</typeparam>
    /// <param name="member">The member information to look up.</param>
    /// <param name="predicate">A function that tests each rule.</param>
    /// <returns>
    /// <see langword="true"/> as soon as the first rule satisfies the 
    /// <paramref name="predicate"/>; otherwise <see langword="false"/>.
    /// </returns>
    public virtual bool ContainsAny<T>(MemberInfo member, Func<ConditionalValidationRule, bool> predicate)
    {
        var rules = GetRules(member);
        foreach (var rule in rules)
        {
            if (predicate(rule)) return true;
        }
        return false;
    }

    /// <summary>
    /// Finds all rules for a given member access expression.
    /// </summary>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="predicate">Optional rule filter.</param>
    /// <returns>A read-only list of matching rules.</returns>
    public IReadOnlyList<ConditionalValidationRule> FindRules(Expression expression, Func<ConditionalValidationRule, bool>? predicate = null)
        => FindRules(expression.GetMemberInfo(), predicate);

    /// <summary>
    /// Finds all rules for a given <see cref="MemberInfo"/> and optional filter.
    /// </summary>
    /// <param name="member">The member to look up.</param>
    /// <param name="predicate">Optional rule filter.</param>
    /// <returns>A read-only list of matching rules.</returns>
    public virtual IReadOnlyList<ConditionalValidationRule> FindRules(MemberInfo member, Func<ConditionalValidationRule, bool>? predicate = null)
    {
        var rules = GetRules(member);
        return [.. rules.Where(r => member.AreSameMembers(r.Member) && (predicate?.Invoke(r) ?? true))];
    }

    /// <summary>
    /// Returns all rules associated with members declared in the specified type <typeparamref name="T"/>.
    /// This is useful for introspection and diagnostics.
    /// </summary>
    /// <typeparam name="T">The declaring type to filter by.</typeparam>
    /// <returns>A list of tuples (MemberInfo, Rule List) for each matched member.</returns>
    public virtual List<(MemberInfo Member, List<ConditionalValidationRule> Rules)> EnumerateRules<T>()
        => EnumerateRules(typeof(T));

    /// <summary>
    /// Enumerates all validation rules that apply to the specified type, organizing them by member.
    /// The result is cached for subsequent lookups to improve performance.
    /// </summary>
    /// <param name="type">The type to enumerate rules for.</param>
    /// <returns>
    /// A list of tuples, where each tuple contains a <see cref="MemberInfo"/> and a list of
    /// <see cref="ConditionalValidationRule"/> that apply to that member for the given type.
    /// </returns>
    public virtual List<(MemberInfo Member, List<ConditionalValidationRule> Rules)> EnumerateRules(Type type)
    {
        var result = new List<(MemberInfo, List<ConditionalValidationRule>)>();
        foreach (var (member, rules) in _ruleRegistry)
        {
            if (IsAssignableFrom(member.ReflectedType, type))
            {
                result.Add((member, rules.ToList()));
            }
        }
        return result;
    }

    /// <inheritdoc />
    public virtual List<ConditionalValidationRule> GetRulesForType(Type type)
        => [.. EnumerateRules(type).SelectMany(list => list.Rules)];

    /// <summary>
    /// Removes all rules registered for the specified member.
    /// </summary>
    /// <param name="member">The member whose rules should be removed.</param>
    /// <returns>True if any rules were removed, false otherwise.</returns>
    public bool RemoveAll(MemberInfo member)
    {
        return member.DeclaringType != null
            ? RemoveAllForType(member.DeclaringType, m => m.Name == member.Name) > 0
            : _ruleRegistry.Remove(member, out _);
    }

    /// <summary>
    /// Removes all rules associated with the specified type.
    /// </summary>
    /// <param name="type">The type whose members' rules should be removed.</param>
    /// <param name="predicate">
    /// An optional function that determines whether a key should be 
    /// removed from the registry when a compatible type is matched.
    /// </param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAllForType(Type type, Func<MemberInfo, bool>? predicate = null)
    {
        int removedCount = 0;
        foreach (var member in GetMembers())
        {
            if (IsAssignableFrom(member.DeclaringType, type) &&
                (predicate is null || predicate(member)) &&
                _ruleRegistry.TryRemove(member, out _)
            )
            {
                removedCount++;
            }
        }
        return removedCount;
    }

    /// <summary>
    /// Removes all rules matching the specified predicate from all members.
    /// </summary>
    /// <param name="predicate">
    /// A function that takes a MemberInfo and returns true if the rule should be removed.
    /// </param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll(Func<MemberInfo, bool> predicate)
    {
        int removedCount = 0;
        foreach (var member in GetMembers())
        {
            if (predicate(member) && _ruleRegistry.TryRemove(member, out _))
                removedCount++;
        }
        return removedCount;
    }

    /// <summary>
    /// Removes all rules associated with the specified attribute type for a given member.
    /// </summary>
    /// <typeparam name="TAttribute">The type of validation attribute to filter by.</typeparam>
    /// <param name="key">The member whose rules should be removed.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll<TAttribute>(MemberInfo key) where TAttribute : ValidationAttribute
        => RemoveAll<TAttribute>((member, _) => key.AreSameMembers(member));

    /// <summary>
    /// Removes all rules associated with the specified attribute type for a given member.
    /// </summary>
    /// <param name="key">The member whose rules should be removed.</param>
    /// <param name="attributeType">The type of validation attribute to filter by.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll(MemberInfo key, Type attributeType)
    {
        int removedCount = 0;
        var members = GetMembers(key.AreSameMembers);
        foreach (var member in members)
        {
            if (!_ruleRegistry.TryGetValue(member, out var rules))
                continue;

            // Use pooled list to reduce allocations if performance-critical
            var updatedRules = new List<ConditionalValidationRule>(rules.Count);

            foreach (var rule in rules)
            {
                if (rule.Attribute is { } attr && attr.GetType() == attributeType)
                {
                    removedCount++;
                    continue;
                }
                updatedRules.Add(rule);
            }

            if (removedCount > 0)
                _ruleRegistry[member] = updatedRules;
        }
        return removedCount;
    }

    /// <summary>
    /// Removes all rules matching the specified predicate for a given attribute type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of validation attribute to filter by.</typeparam>
    /// <param name="predicate">A function that takes a MemberInfo and an attribute instance.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAll<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate) where TAttribute : ValidationAttribute
    {
        int removedCount = 0;
        foreach (var member in GetMembers())
        {
            if (!_ruleRegistry.TryGetValue(member, out var rules))
                continue;

            // Use pooled list to reduce allocations if performance-critical
            var updatedRules = new List<ConditionalValidationRule>(rules.Count);

            foreach (var rule in rules)
            {
                if (rule.Attribute is TAttribute attr && predicate(member, attr))
                {
                    removedCount++;
                    continue;
                }
                updatedRules.Add(rule);
            }

            if (removedCount > 0)
                _ruleRegistry[member] = updatedRules;
        }
        return removedCount;
    }

    /// <summary>
    /// Gets a deep copy of the internal rule registry for the specified member.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> of the member to retrieve.</param>
    /// <remarks>
    /// This method provides a snapshot of the current state of the rule registry,
    /// preventing external code from directly modifying the validator's internal configuration.
    /// </remarks>
    /// <returns>A new <see cref="ConcurrentDictionary{TKey, TValue}"/> containing copies
    /// of the member-rule mappings.</returns>
    protected internal ConcurrentDictionary<MemberInfo, List<ConditionalValidationRule>> GetRegistryForMember(MemberInfo member)
    {
        // Returns a new ConcurrentDictionary with deep-copied lists of rules.
        return new ConcurrentDictionary<MemberInfo, List<ConditionalValidationRule>>(
            _ruleRegistry.Where(kvp => member.AreSameMembers(kvp.Key))
            .Select(kvp =>
                new KeyValuePair<MemberInfo, List<ConditionalValidationRule>>(
                    kvp.Key,
                    [.. kvp.Value]
                )
            )
        );
    }

    private List<MemberInfo> GetMembers(Func<MemberInfo, bool>? predicate = null) =>
        predicate is null
            ? [.. _ruleRegistry.Keys]
            : [.. _ruleRegistry.Keys.Where(predicate)];
}
