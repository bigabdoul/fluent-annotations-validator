using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Resolves a synthesized ConditionalValidationRule when explicit configuration is missing.
/// Combines global culture, resource type, and conventional key fallback.
/// </summary>
public interface IImplicitRuleResolver
{
    /// <summary>
    /// Resolves a contextual rule for the given DTO type, property, and attribute.
    /// </summary>
    /// <param name="dtoType">DTO type (e.g., LoginDto)</param>
    /// <param name="property">DTO property (e.g., Password)</param>
    /// <param name="attribute">ValidationAttribute instance (e.g., RequiredAttribute)</param>
    /// <returns>Fallback ConditionalValidationRule with inferred metadata</returns>
    ConditionalValidationRule Resolve(Type dtoType, PropertyInfo property, ValidationAttribute attribute, ValidationBehaviorOptions? options = null);
}
