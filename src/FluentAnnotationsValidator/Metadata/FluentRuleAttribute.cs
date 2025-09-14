using FluentAnnotationsValidator.Abstractions;
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
        var validator = CreateValidator(ObjectType);
        var result = validator.Validate(validationContext);
        return GetValidationResult(result.Errors);
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
        var validator = CreateValidator(ObjectType);
        var result = await validator.ValidateAsync(validationContext, cancellationToken);
        return GetValidationResult(result.Errors);
    }
}