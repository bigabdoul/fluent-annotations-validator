using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Annotations;

using Core.Interfaces;
using Core.Results;

/// <summary>
/// Specifies that a set of reusable validation rules should be applied to a member.
/// The rules are sourced from a class that has been configured with the validator.
/// </summary>
/// <remarks>
/// This attribute enables the reuse of complex validation logic by referencing a type
/// that contains the configured rules.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="FluentRuleAttribute"/> class.
/// </remarks>
/// <param name="objectType">The type that holds the validation rules.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public class FluentRuleAttribute(Type objectType) : FluentValidationAttribute, IFluentRuleAttribute
{
    /// <summary>
    /// Gets the type that contains the configured validation rules to be applied.
    /// </summary>
    public Type ObjectType { get; } = objectType ?? throw new ArgumentNullException(nameof(objectType));

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(value);

        var registry = RuleRegistry;
        var resolver = MessageResolver;

        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(validationContext.MemberName);

        var member = validationContext.ObjectType.GetMember(validationContext.MemberName)[0];
        var memberRules = registry.GetRulesByMember(ObjectType).Where(g => string.Equals(member.Name, g.Key.Name));
        var items = validationContext.Items;

        foreach (var rules in memberRules)
        {
            var results = rules.Validate(value, member, resolver, registry, items);
            if (results.Count > 0)
            {
                Errors.AddRange(results.Select(e => new FluentValidationFailure(e)));
            }
        }

        return Errors.Count == 0
            ? ValidationResult.Success :
            GetFailedValidationResult(this, validationContext, MessageResolver);
    }
}
