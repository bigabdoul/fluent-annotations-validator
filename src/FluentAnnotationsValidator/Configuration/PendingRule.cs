using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a temporary rule being configured for a given type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type containing the member being being configured.</typeparam>
/// <param name="expression">The member being configured.</param>
/// <param name="predicate">A function that evaluates when the rule is applied.</param>
/// <param name="message">The validation error message.</param>
/// <param name="key">The failure key used by the message resolver or diagnostics.</param>
/// <param name="resourceKey">The resource manager's key for retrieving a localized error message.</param>
/// <param name="resourceType">The resource manager's type for localized error message.</param>
/// <param name="culture">The culture to use in the resource manager.</param>
/// <param name="fallbackMessage">A fallback error message if resolution fails.</param>
/// <param name="useConventionalKeys">Indicates whether to use convention-based resource key names (e.g., Email_Required).</param>
public sealed class PendingRule<T>(
    Expression expression,
    Predicate<T> predicate = default!,
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool? useConventionalKeys = true
) : ValidationRule<T>(predicate, expression ?? throw new ArgumentNullException(nameof(expression)), message, key, resourceKey, resourceType, culture, fallbackMessage, useConventionalKeys)
{
    /// <summary>
    /// Gets or sets the expression of the member being configured.
    /// </summary>
    public new Expression Expression { get => base.Expression!; set => base.Expression = value; }

    /// <summary>
    /// Gets or sets a function that evaluates when the rule is applied asynchronously.
    /// </summary>
    public new Func<T, CancellationToken, Task<bool>>? AsyncCondition { get; set; }

    /// <summary>
    /// Gets the list of dynamically added attributes via fluent rules.
    /// </summary>
    public List<ValidationAttribute> Attributes { get; } = [];

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString() =>
        $"Member: {Expression?.GetMemberInfo().Name} | Attributes ({Attributes.Count}): " +
        string.Join(", ", Attributes.Select(a => $"[{a.GetType().Name}]"));
}