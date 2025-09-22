using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Results;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

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
public class FluentRuleAttribute(Type objectType) : FluentValidationAttribute
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
            this.GetFailedValidationResult(validationContext);
    }
}

/// <summary>
/// Specifies that a set of reusable validation rules should be applied to a member asynchronously.
/// The rules are sourced from a class that has been configured with the validator.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluentRuleAsyncAttribute"/> class.
/// </remarks>
/// <param name="objectType">The type that holds the validation rules.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
public class FluentRuleAsyncAttribute(Type objectType) : FluentRuleAttribute(objectType), IAsyncValidationAttribute
{
    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        throw new InvalidOperationException("The attribute supports only asynchronous validation. " +
            $"Invoke the {nameof(ValidateAsync)} method.");
    }

    /// <inheritdoc/>
    public async Task<ValidationResult?> ValidateAsync(object? value, ValidationContext validationContext, CancellationToken cancellationToken)
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
            var results = await rules.ValidateAsync(value, member, resolver, registry, items, cancellationToken);
            if (results.Count > 0)
            {
                Errors.AddRange(results.Select(e => new FluentValidationFailure(e)));
            }
        }

        return Errors.Count == 0
            ? ValidationResult.Success :
            this.GetFailedValidationResult(validationContext);
    }
}