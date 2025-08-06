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
public sealed class ConditionalValidationRule(
    Func<object, bool> Predicate,
    string? Message = null,
    string? Key = null,
    string? ResourceKey = null,
    Type? ResourceType = null,
    System.Globalization.CultureInfo? Culture = null,
    string? FallbackMessage = null,
    bool UseConventionalKeyFallback = true)
{
    public Func<object, bool> Predicate { get; set; } = Predicate;
    public string? Message { get; set; } = Message;
    public string? Key { get; set; } = Key;
    public string? ResourceKey { get; set; } = ResourceKey;
    public Type? ResourceType { get; set; } = ResourceType;
    public CultureInfo? Culture { get; set; } = Culture;
    public string? FallbackMessage { get; set; } = FallbackMessage;
    public bool UseConventionalKeyFallback { get; set; } = UseConventionalKeyFallback;

    /// <summary>
    /// The validation attribute associated to the rule.
    /// If it is <see cref="null"/>, it may have been added
    /// through fluent configuration.
    /// </summary>
    public ValidationAttribute? Attribute { get; set; }
    public MemberInfo Member { get; init; } = default!;
    public bool ShouldApply(object targetInstance) => Predicate(targetInstance);

    /// <summary>
    /// Indicates whether the <see cref="Attribute"/> property is not <see langword="null"/>.
    /// If the return value is <see langword="true"/>, it likely has been added
    /// via fluent configuration.
    /// </summary>
    public bool HasAttribute => Attribute != null;

    public string UniqueKey { get; set; } = Guid.NewGuid().ToString();

    public override bool Equals(object? obj)
        => obj is ConditionalValidationRule other && Equals(other);

    public bool Equals(ConditionalValidationRule? other)
        => other is not null &&
           Member.Name == other.Member.Name &&
           Member.DeclaringType == other.Member.DeclaringType &&
           Attribute?.GetType() == other.Attribute?.GetType();

    public override int GetHashCode()
        => HashCode.Combine(Member.Name, Member.DeclaringType, Attribute?.GetType());
}
