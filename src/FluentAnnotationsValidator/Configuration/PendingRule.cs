using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a temporary rule being configured for a given type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type containing the member being being configured.</typeparam>
/// <param name="memberExpression">The member being configured.</param>
/// <param name="predicate">A function that evaluates when the rule is applied.</param>
/// <param name="message">The validation error message.</param>
/// <param name="key">The failure key used by the message resolver or diagnostics.</param>
/// <param name="resourceKey">The resource manager's key for retrieving a localized error message.</param>
/// <param name="resourceType">The resource manager's type for localized error message.</param>
/// <param name="culture">The culture to use in the resource manager.</param>
/// <param name="fallbackMessage">A fallback error message if resolution fails.</param>
/// <param name="useConventionalKeys">Indicates whether to use convention-based resource key names (e.g., Email_Required).</param>
public sealed class PendingRule<T>(
    Expression memberExpression,
    Func<T, bool> predicate,
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool? useConventionalKeys = true
) : ValidationRuleBase(message, key, resourceKey, resourceType, culture, fallbackMessage, useConventionalKeys)
{
    /// <summary>
    /// Gets or sets the member being configured.
    /// </summary>
    public Expression MemberExpression { get; set; } = memberExpression;

    /// <summary>
    /// Gets or sets a function that evaluates when the rule is applied.
    /// </summary>
    public Func<T, bool> Predicate { get; set; } = predicate;

    /// Gets the list of dynamically added attributes via fluent rules.
    /// </summary>
    public List<ValidationAttribute> Attributes { get; } = [];

    public override string ToString() => 
        $"Member: {MemberExpression.GetMemberInfo().Name} | Attributes ({Attributes.Count}): " +
        string.Join(", ", Attributes.Select(a => $"[{a.GetType().Name}]"));

    public override int GetHashCode() => MemberExpression.GetMemberInfo().GetHashCode();

    public override bool Equals(object? obj) => obj is PendingRule<T> other && Equals(other);

    public bool Equals(PendingRule<T>? other) => 
        other != null && MemberExpression.GetMemberInfo().AreSameMembers(other.MemberExpression.GetMemberInfo());
}