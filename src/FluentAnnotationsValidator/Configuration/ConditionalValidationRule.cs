using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a conditional validation rule that applies a predicate function 
/// to determine whether a validation constraint should be enforced on a property.
/// </summary>
/// <param name="Predicate">
/// A delegate that takes the model instance and returns <see langword="true"/> if the condition is met;
/// otherwise, <see langword="false"/>. This is used to conditionally trigger validation logic.
/// </param>
/// <param name="Message">
/// An optional custom error message to display when the validation fails.
/// </param>
/// <param name="Key">
/// An optional key to identify the rule, which can be used for logging, debugging, or tracking.
/// </param>
/// <param name="ResourceKey">
/// An optional localization resource key to resolve the validation message.
/// </param>
/// <param name="ResourceType">
/// An optional localization resource type to resolve the validation message.
/// </param>
/// <param name="Culture">An optional culture-specific format provider.</param>
/// <param name="FallbackMessage">Specifies a message to fall back to if .Localized(...) lookup fails - avoids silent runtime fallback.</param>
/// <param name="UseConventionalKeyFallback">Explicitly disables "Property_Attribute" fallback lookup - for projects relying solely on .WithKey(...).</param>
public class ConditionalValidationRule(
    Func<object, bool> Predicate,
    string? Message = null,
    string? Key = null,
    string? ResourceKey = null,
    Type? ResourceType = null,
    CultureInfo? Culture = null,
    string? FallbackMessage = null,
    bool UseConventionalKeyFallback = true)
{
    private Func<object, bool>? _shouldApplyEvaluator;

    /// <summary>
    /// Gets or sets a function that evaluates when the rule is applied.
    /// </summary>
    public Func<object, bool> Predicate { get; set; } = Predicate;

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? Message { get; set; } = Message;

    /// <summary>
    /// Gets or sets the failure key used by the message resolver or diagnostics.
    /// </summary>
    public string? Key { get; set; } = Key;

    /// <summary>
    /// Gets or sets the resource manager's key for retrieving a localized error message.
    /// </summary>
    public string? ResourceKey { get; set; } = ResourceKey;

    /// <summary>
    /// Gets or sets the resource manager's type for localized error message.
    /// </summary>
    public Type? ResourceType { get; set; } = ResourceType;

    /// <summary>
    /// Gets or sets the culture to use in the resource manager.
    /// </summary>
    public CultureInfo? Culture { get; set; } = Culture;

    /// <summary>
    /// Gets or sets a fallback error message if resolution fails.
    /// </summary>
    public string? FallbackMessage { get; set; } = FallbackMessage;

    /// <summary>
    /// Gets or sets a value that indicates whether to use convention-based 
    /// resource key names (e.g., Email_Required).
    /// </summary>
    public bool UseConventionalKeyFallback { get; set; } = UseConventionalKeyFallback;

    /// <summary>
    /// The validation attribute associated to the rule.
    /// If it is <see cref="null"/>, it may have been added
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
    /// Indicates whether the <see cref="Attribute"/> property is not <see langword="null"/>.
    /// If the return value is <see langword="true"/>, it likely has been added via fluent configuration.
    /// </summary>
    public bool HasAttribute => Attribute != null;

    /// <summary>
    /// Gets or sets the unique key of the current rule.
    /// </summary>
    public string UniqueKey { get; set; } = Guid.NewGuid().ToString();

    public override bool Equals(object? obj)
        => obj is ConditionalValidationRule other && Equals(other);

    public bool Equals(ConditionalValidationRule? other) => 
        other is not null &&
        Member.AreSameMembers(other.Member) &&
        Attribute?.GetType() == other.Attribute?.GetType();

    public override int GetHashCode()
        => HashCode.Combine(Member.Name, Attribute?.GetType());

    public override string? ToString() => 
        (HasAttribute ? $"[{Attribute?.GetType().Name}]" : string.Empty) + 
        $"{Member.ReflectedType}.{Member.Name}";
}
