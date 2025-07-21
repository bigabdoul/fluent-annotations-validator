using FluentAnnotationsValidator.Internals.Reflection;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Interfaces;

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
}
