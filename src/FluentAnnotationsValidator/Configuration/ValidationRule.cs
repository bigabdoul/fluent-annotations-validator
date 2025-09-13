using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a validation rule.
/// </summary>
/// <param name="condition">
/// A delegate that takes the model instance and returns <see langword="true"/> if the condition is met;
/// otherwise, <see langword="false"/>. This is used to conditionally trigger validation logic.
/// </param>
/// <param name="expression">The member expression this rule applies to.</param>
/// <param name="message">
/// An optional custom error message to display when the validation fails.
/// </param>
/// <param name="key">
/// An optional key to identify the rule, which can be used for logging, debugging, or tracking.
/// </param>
/// <param name="resourceKey">
/// An optional localization resource key to resolve the validation message.
/// </param>
/// <param name="resourceType">
/// An optional localization resource type to resolve the validation message.
/// </param>
/// <param name="culture">An optional culture-specific format provider.</param>
/// <param name="fallbackMessage">Specifies a message to fall back to if .Localized(...) lookup fails - avoids silent runtime fallback.</param>
/// <param name="useConventionalKeys">Explicitly disables "Property_Attribute" fallback lookup - for projects relying solely on .WithKey(...).</param>
public class ValidationRule(Expression? expression = null, Predicate<object>? condition = null,
string? message = null,
string? key = null,
string? resourceKey = null,
Type? resourceType = null,
CultureInfo? culture = null,
string? fallbackMessage = null,
bool? useConventionalKeys = true) : IValidationRule
{
    private static readonly Predicate<object> DefaultCondition = _ => true;

    /// <inheritdoc />
    public virtual Predicate<object> Condition { get; set; } = condition ?? DefaultCondition;

    /// <inheritdoc />
    public virtual Func<object, CancellationToken, Task<bool>>? AsyncCondition { get; set; }

    /// <inheritdoc />
    public Expression? Expression { get; set; } = expression;

    /// <inheritdoc />
    public MemberInfo Member { get; set; } = expression?.GetMemberInfo()!;

    /// <inheritdoc />
    public ValidationAttribute? Validator { get; set; }

    /// <inheritdoc />
    public string? Message { get; set; } = message;

    /// <inheritdoc />
    public Func<object, string>? MessageResolver { get; set; }

    /// <inheritdoc />
    public string? PropertyName { get; set; }

    /// <inheritdoc />
    public string? Key { get; set; } = key;

    /// <summary>
    /// Gets or sets the unique key of the current rule.
    /// </summary>
    public string UniqueKey { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public string? ResourceKey { get; set; } = resourceKey;

    /// <inheritdoc />
    public Type? ResourceType { get; set; } = resourceType;

    /// <inheritdoc />
    public CultureInfo? Culture { get; set; } = culture;

    /// <inheritdoc />
    public string? FallbackMessage { get; set; } = fallbackMessage;

    /// <inheritdoc />
    public PreValidationValueProviderDelegate? ConfigureBeforeValidation { get; set; }

    /// <inheritdoc />
    public bool? UseConventionalKeys { get; set; } = useConventionalKeys;

    /// <inheritdoc/>
    public bool HasValidator => Validator != null;

    /// <inheritdoc/>
    public string GetPropertyName() => string.IsNullOrWhiteSpace(PropertyName) ? Member.Name : PropertyName;

    /// <summary>
    /// When overridden, determines whether the current rule applies to <paramref name="targetInstance"/>.
    /// </summary>
    /// <param name="targetInstance">The target object instance.</param>
    /// <returns><see langword="true"/> if the current rule should be validated; otherwise, <see langword="false"/>.</returns>
    public virtual bool ShouldValidate(object targetInstance) => Condition(targetInstance);

    /// <summary>
    /// When overridden, determines whether the current rule 
    /// applies to <paramref name="targetInstance"/> asynchronously.
    /// </summary>
    /// <param name="targetInstance">The target object instance.</param>
    /// <param name="cancellationToken">An object that propagates notificiation that operations should be canceled.</param>
    /// <returns><see langword="true"/> if the current rule should be validated; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public virtual Task<bool> ShouldValidateAsync(object targetInstance, CancellationToken cancellationToken)
        => AsyncCondition is null ? Task.FromResult(true) : AsyncCondition(targetInstance, cancellationToken);
}