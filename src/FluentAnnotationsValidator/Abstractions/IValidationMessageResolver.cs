using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// A contract that defines what it means to resolve messages across resource strategies.
/// </summary>
public interface IValidationMessageResolver
{
    /// <summary>
    /// Resolves the error message for a given validation attribute, member, and rule context.
    /// </summary>
    /// <typeparam name="T">The type on which the member is declared.</typeparam>
    /// <param name="expression">A lambda expression used to extract the declaring type and member info.</param>
    /// <param name="attr">The validation attribute being processed</param>
    /// <param name="rule">An optional conditional validation rule to use.</param>
    /// <returns>Localized error message or null</returns>
    string? ResolveMessage<T>(Expression<Func<T, string?>> expression, ValidationAttribute attr, ConditionalValidationRule? rule = null);

    /// <summary>
    /// Resolves the error message for a given validation attribute, member, and rule context.
    /// </summary>
    /// <param name="declaringType">The type on which the member is declared.</param>
    /// <param name="memberName">The property or field name to which the attribute is attached.</param>
    /// <param name="attr">The validation attribute being processed</param>
    /// <param name="rule">An optional conditional validation rule to use.</param>
    /// <returns>Localized error message or null</returns>
    string? ResolveMessage(Type declaringType, string memberName, ValidationAttribute attr, ConditionalValidationRule? rule = null);
}
