using FluentAnnotationsValidator.Abstractions;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Resolves a synthesized ConditionalValidationRule when explicit configuration is missing.
/// Combines global culture, resource type, and conventional key fallback.
/// </summary>
/// <param name="options"></param>
public sealed class ImplicitRuleResolver(IOptions<ValidationBehaviorOptions> options) : IImplicitRuleResolver
{
    private readonly ValidationBehaviorOptions _options = options.Value;

    /// <inheritdoc />
    public ConditionalValidationRule Resolve(Type dtoType, PropertyInfo property,
        ValidationAttribute attribute, ValidationBehaviorOptions? options = null)
    {
        // Prefer explicitly registered rule
        if ((options ?? _options).TryGet(dtoType, property.Name, out var rule))
            return rule;

        // Attempt to resolve configurator for this type
        _ = ValidationConfiguratorStore.Instance.TryGet(dtoType, out var configurator);

        // Infer conventional resource key like "Password_Required"
        var conventionalKey = $"{property.Name}_{attribute.GetType().Name.Replace("Attribute", "")}";

        if (configurator != null && configurator.Rules.TryGetValue(property.Name, out var storedRule))
        {
            rule = storedRule;
        }
        else
        {
            rule = new ConditionalValidationRule(
                Predicate: _ => true,
                Key: null,
                ResourceKey: conventionalKey,
                ResourceType: configurator?.ValidationResourceType,
                Culture: configurator?.Culture,
                FallbackMessage: null,
                UseConventionalKeyFallback: true
            );

            if (configurator != null)
                configurator.Rules[property.Name] = rule;
        }

        return rule;
    }
}
