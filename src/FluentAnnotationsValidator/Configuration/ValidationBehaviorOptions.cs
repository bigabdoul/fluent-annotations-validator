using FluentAnnotationsValidator.Extensions;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Central configuration point for FluentAnnotationsValidator.
/// Stores conditional validation rules mapped to each property or field.
/// Supports multiple validation attributes per member.
/// </summary>
public class ValidationBehaviorOptions
{
    private static readonly InvalidOperationException NoMatchingRule =
        new("Found no rule matching the specified expression.");

    private readonly ConcurrentDictionary<MemberInfo, List<ConditionalValidationRule>> _ruleRegistry = new();

    #region properties

    /// <summary>
    /// Optional common resource type used for localization.
    /// </summary>
    public Type? CommonResourceType { get; set; }

    /// <summary>
    /// Optional culture to use for error messages and formatting.
    /// </summary>
    public CultureInfo? CommonCulture { get; set; }

    /// <summary>
    /// When true, uses conventional resource key naming (e.g. MemberName_Attribute).
    /// </summary>
    public bool UseConventionalKeys { get; set; } = true;

    /// <summary>
    /// Used when running tests.
    /// </summary>
    public string? CurrentTestName { get; set; }

    #endregion

    internal void AddRules(MemberInfo member, List<ConditionalValidationRule> rules)
    {
        _ruleRegistry.AddOrUpdate(member, rules, (_, _) => rules);
    }

    /// <summary>
    /// Registers a conditional validation rule for the specified member.
    /// Multiple rules can be associated with a single member.
    /// </summary>
    /// <param name="member">The target property or field.</param>
    /// <param name="rule">The conditional validation rule to apply.</param>
    public virtual void AddRule(MemberInfo member, ConditionalValidationRule rule)
    {
        _ruleRegistry.AddOrUpdate(
            member,
            _ => [rule],
            (_, rules) =>
            {
                rules.Add(rule);
                return rules;
            }
        );
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
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="predicate">Optional filter applied to rules.</param>
    /// <returns>True if at least one matching rule exists, false otherwise.</returns>
    public virtual bool Contains<T>(
        Expression<Func<T, string?>> expression,
        Func<ConditionalValidationRule, bool>? predicate = null)
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
    /// Finds all rules for a given member access expression.
    /// </summary>
    /// <typeparam name="T">Declaring type of the property.</typeparam>
    /// <param name="expression">Expression referencing the property.</param>
    /// <param name="predicate">Optional rule filter.</param>
    /// <returns>A read-only list of matching rules.</returns>
    public IReadOnlyList<ConditionalValidationRule> FindRules<T>(
        Expression<Func<T, string?>> expression,
        Func<ConditionalValidationRule, bool>? predicate = null)
        => FindRules<T>(expression.GetMemberInfo(), predicate);

    /// <summary>
    /// Finds all rules for a given <see cref="MemberInfo"/> and optional filter.
    /// </summary>
    /// <typeparam name="T">Declaring type of the member.</typeparam>
    /// <param name="member">The member to look up.</param>
    /// <param name="predicate">Optional rule filter.</param>
    /// <returns>A read-only list of matching rules.</returns>
    public IReadOnlyList<ConditionalValidationRule> FindRules<T>(
        MemberInfo member,
        Func<ConditionalValidationRule, bool>? predicate = null)
        => FindRules(typeof(T), member, predicate);

    public virtual IReadOnlyList<ConditionalValidationRule> FindRules(Type type,
        MemberInfo member, Func<ConditionalValidationRule, bool>? predicate = null)
    {
        var rules = GetRules(member);

        return [.. rules.Where(r =>
                r.Member.DeclaringType == type &&
                r.Member.Name == member.Name &&
                (predicate?.Invoke(r) ?? true))
        ];
    }

    /// <summary>
    /// Returns all rules associated with members declared in the specified type <typeparamref name="T"/>.
    /// This is useful for introspection and diagnostics.
    /// </summary>
    /// <typeparam name="T">The declaring type to filter by.</typeparam>
    /// <returns>A list of tuples (MemberInfo, Rule List) for each matched member.</returns>
    public virtual List<(MemberInfo Member, List<ConditionalValidationRule> Rules)> EnumerateRules<T>()
    {
        var result = new List<(MemberInfo, List<ConditionalValidationRule>)>();

        foreach (var (member, rules) in _ruleRegistry)
        {
            if (member.DeclaringType == typeof(T))
            {
                result.Add((member, rules.ToList()));
            }
        }

        return result;
    }

    /// <summary>
    /// Removes all rules registered for the specified member.
    /// </summary>
    /// <param name="member">The member whose rules should be removed.</param>
    /// <returns>True if any rules were removed, false otherwise.</returns>
    public bool RemoveAll(MemberInfo member) => _ruleRegistry.Remove(member, out var _);
}
