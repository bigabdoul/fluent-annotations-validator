using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a conditional validation rule that applies a predicate function 
/// to determine whether a validation constraint should be enforced on a property.
/// </summary>
/// <param name="predicate">
/// A delegate that takes the model instance and returns <see langword="true"/> if the condition is met;
/// otherwise, <see langword="false"/>. This is used to conditionally trigger validation logic.
/// </param>
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
public class ConditionalValidationRule(
    Func<object, bool> predicate,
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool useConventionalKeys = true) :
    ValidationRuleBase(message, key, resourceKey, resourceType, culture, fallbackMessage, useConventionalKeys)
{
    private Func<object, bool>? _shouldApplyEvaluator;
    private Func<object, CancellationToken, Task<bool>>? _shouldApplyAsyncEvaluator;

    /// <summary>
    /// Gets or sets a function that evaluates when the rule is applied.
    /// </summary>
    public Func<object, bool> Predicate { get; set; } = predicate;

    /// <summary>
    /// Gets or sets a function that evaluates when the rule is applied asynchronously.
    /// </summary>
    public Func<object, CancellationToken, Task<bool>>? AsyncPredicate { get; set; }

    /// <summary>
    /// The validation attribute associated to the rule.
    /// If it is <see langword="null"/>, it may have been added
    /// through fluent configuration.
    /// </summary>
    public ValidationAttribute? Attribute { get; set; }

    /// <summary>
    /// Gets or sets the member this rule applies to.
    /// </summary>
    public MemberInfo Member { get; init; } = default!;

    /// <summary>
    /// Determines whether the current rule should be evaluated.
    /// </summary>
    /// <param name="targetInstance">The target instance passed to the predicate.</param>
    /// <returns>
    /// <see langword="true"/> if the rule should be evaluated; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool ShouldApply(object targetInstance) =>
        (_shouldApplyEvaluator ?? Predicate)(targetInstance);

    /// <summary>
    /// Sets the function used to evaluate rule application.
    /// </summary>
    /// <param name="predicate">
    /// A function used to evaluate whether the current rule should be applied.
    /// </param>
    public void SetShouldApply(Func<object, bool> predicate) =>
        _shouldApplyEvaluator = predicate;

    /// <summary>
    /// Asynchronously determines whether the current rule should be evaluated.
    /// </summary>
    /// <param name="targetInstance">The target instance passed to the predicate.</param>
    /// <param name="cancellationToken">An object that propagates notification that operations should be canceled.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <see langword="true"/> if the rule should be evaluated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual Task<bool> ShouldApplyAsync(object targetInstance, CancellationToken cancellationToken = default)
    {
        var asyncPredicate = AsyncPredicate;

        if (_shouldApplyAsyncEvaluator is null && asyncPredicate is null)
            return Task.Run(() => ShouldApply(targetInstance));

        return _shouldApplyAsyncEvaluator is not null
            ? _shouldApplyAsyncEvaluator(targetInstance, cancellationToken)
            : asyncPredicate!(targetInstance, cancellationToken);
    }

    /// <summary>
    /// Sets the asynchronous function used to evaluate rule application.
    /// </summary>
    /// <param name="predicate">
    /// An asynchronous function used to evaluate whether the current rule should be applied.
    /// </param>
    public void SetShouldApplyAsync(Func<object, CancellationToken, Task<bool>> predicate)
        => _shouldApplyAsyncEvaluator = predicate;

    /// <summary>
    /// Indicates whether the <see cref="Attribute"/> property is not <see langword="null"/>.
    /// If the return value is <see langword="true"/>, it likely has been added via fluent configuration.
    /// </summary>
    public bool HasAttribute => Attribute != null;

    /// <summary>
    /// Gets or sets the unique key of the current rule.
    /// </summary>
    public string UniqueKey { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc cref="object.Equals(object?)"/>
    public override bool Equals(object? obj)
        => obj is ConditionalValidationRule other && Equals(other);

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
    public virtual bool Equals(ConditionalValidationRule? other) =>
        other is not null &&
        Member.AreSameMembers(other.Member) &&
        Attribute?.GetType() == other.Attribute?.GetType();

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode()
        => HashCode.Combine(Member.Name, Attribute?.GetType());

    /// <inheritdoc cref="object.ToString"/>
    public override string? ToString() =>
        (HasAttribute ? $"[{Attribute?.GetType().Name}]" : string.Empty) +
        $"{Member.ReflectedType}.{Member.Name}";
}
