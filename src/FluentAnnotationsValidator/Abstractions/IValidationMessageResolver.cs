using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// A contract for services that can resolve and format localized validation messages.
/// </summary>
/// <remarks>
/// This service is used by the validation engine to generate user-friendly error messages
/// for both attribute-based and fluent-configured validation failures.
/// </remarks>
public interface IValidationMessageResolver
{
    /// <summary>
    /// Resolves and formats a validation message based on the provided validation metadata.
    /// </summary>
    /// <param name="declaringType">The type (class) that declares the validated member.</param>
    /// <param name="memberName">The name of the validated property or field.</param>
    /// <param name="attr">The <see cref="ValidationAttribute"/> instance that failed validation.</param>
    /// <param name="rule">
    /// An optional <see cref="ConditionalValidationRule"/> containing metadata for a fluent-configured rule.
    /// This is used to resolve messages when a rule is defined via the fluent API rather than a static attribute.
    /// </param>
    /// <returns>
    /// A formatted validation message string, or <c>null</c> if no message could be resolved.
    /// </returns>
    string? ResolveMessage(Type declaringType, string memberName, ValidationAttribute attr, ConditionalValidationRule? rule = null);
}