﻿using System.ComponentModel.DataAnnotations;
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
public record ConditionalValidationRule(
    Func<object, bool> Predicate,
    string? Message = null,
    string? Key = null,
    string? ResourceKey = null,
    Type? ResourceType = null,
    System.Globalization.CultureInfo? Culture = null,
    string? FallbackMessage = null,
    bool UseConventionalKeyFallback = true)
{
    /// <summary>
    /// The validation attribute associated to the rule.
    /// If it is <see cref="null"/>, it may have been added
    /// through fluent configuration.
    /// </summary>
    public ValidationAttribute? Attribute { get; set; }
    public MemberInfo Member { get; set; } = default!;
    public bool ShouldApply(object targetInstance) => Predicate(targetInstance);

    /// <summary>
    /// Indicates whether the <see cref="Attribute"/> property is not <see langword="null"/>.
    /// If the return value is <see langword="true"/>, it likely has been added
    /// via fluent configuration.
    /// </summary>
    public bool HasAttribute => Attribute != null;

    public string UniqueKey { get; set; } = Guid.NewGuid().ToString();
}
