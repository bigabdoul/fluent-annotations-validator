using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// A contract that defines what it means to resolve messages across resource strategies.
/// </summary>
public interface IValidationMessageResolver
{
    /// <summary>
    /// Resolves the error message for a given validation attribute and property context.
    /// </summary>
    /// <param name="propertyInfo">Property and attribute metadata</param>
    /// <param name="attr">The validation attribute being processed</param>
    /// <returns>Localized error message or null</returns>
    string? ResolveMessage(PropertyValidationInfo propertyInfo, ValidationAttribute attr);

    /// <summary>
    /// Resolves the error message for a given validation attribute, property, and rule context.
    /// </summary>
    /// <param name="propertyInfo">Property and attribute metadata</param>
    /// <param name="attr">The validation attribute being processed</param>
    /// <param name="rule">An optional conditional validation rule to use.</param>
    /// <returns>Localized error message or null</returns>
    string? ResolveMessage(PropertyValidationInfo propertyInfo, ValidationAttribute attr, ConditionalValidationRule? rule);
}
