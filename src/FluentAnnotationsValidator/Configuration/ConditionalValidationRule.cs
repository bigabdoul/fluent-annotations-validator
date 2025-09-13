using System.Globalization;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a conditional validation rule that applies a predicate function 
/// to determine whether a validation constraint should be enforced on a property.
/// </summary>
/// <param name="expression">The member expression this rule applies to.</param>
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
[Obsolete("Use " + nameof(ValidationRule<T>), true)]
public class ConditionalValidationRule<T>(
    Predicate<T> predicate,
    Expression? expression = null,
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool useConventionalKeys = true) :
    ValidationRule<T>(predicate, expression, message, key, resourceKey, resourceType, culture, fallbackMessage, useConventionalKeys)
{
}
