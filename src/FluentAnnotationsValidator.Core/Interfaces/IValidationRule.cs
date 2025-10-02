using FluentAnnotationsValidator.Core;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Core.Interfaces;

/// <summary>
/// Defines a contract for untyped validation rule metadata and evaluation logic.
/// </summary>
/// <remarks>
/// This interface supports rule configuration, conditional evaluation, localization, and diagnostics.
/// It is typically used internally by the validation engine or for reflection-based rule traversal.
/// </remarks>
public interface IValidationRule
{
    /// <summary>
    /// Gets or sets a predicate that determines whether the rule should be applied to a given instance.
    /// </summary>
    Predicate<object> Condition { get; set; }

    /// <summary>
    /// Gets or sets an asynchronous predicate that determines whether the rule should be applied.
    /// </summary>
    Func<object, CancellationToken, Task<bool>>? AsyncCondition { get; set; }

    /// <summary>
    /// Gets or sets the validation attribute associated with the rule.
    /// </summary>
    /// <remarks>
    /// If <see langword="null"/>, the rule may have been added through fluent configuration.
    /// </remarks>
    ValidationAttribute? Validator { get; set; }

    /// <summary>
    /// Gets or sets a delegate used to configure the target instance before validation occurs.
    /// </summary>
    PreValidationValueProviderDelegate? ConfigureBeforeValidation { get; set; }

    /// <summary>
    /// Gets or sets the culture used for localization and formatting.
    /// </summary>
    CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets a fallback error message used if localization or key resolution fails.
    /// </summary>
    string? FallbackMessage { get; set; }

    /// <summary>
    /// Gets or sets the diagnostic failure key used for message resolution or logging.
    /// </summary>
    string? Key { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the rule.
    /// </summary>
    string UniqueKey { get; set; }

    /// <summary>
    /// Gets or sets the lambda expression representing the member being validated.
    /// </summary>
    Expression? Expression { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="MemberInfo"/> of the property or field being validated.
    /// </summary>
    MemberInfo Member { get; set; }

    /// <summary>
    /// Gets or sets the error message displayed when validation fails.
    /// </summary>
    string? Message { get; set; }

    /// <summary>
    /// Gets or sets a delegate that dynamically resolves the error message.
    /// </summary>
    Func<object, string>? MessageResolver { get; set; }

    /// <summary>
    /// Gets or sets an overridden property name used for diagnostics or localization.
    /// </summary>
    string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the resource key used to retrieve a localized error message.
    /// </summary>
    string? ResourceKey { get; set; }

    /// <summary>
    /// Gets or sets the resource type used for localization.
    /// </summary>
    Type? ResourceType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether conventional resource key lookup should be disabled.
    /// </summary>
    /// <remarks>
    /// When <c>false</c>, fallback keys like <c>Property_Attribute</c> may be used.
    /// </remarks>
    bool? UseConventionalKeys { get; set; }

    /// <summary>
    /// Gets a value indicating whether the rule has an associated validation attribute.
    /// </summary>
    /// <remarks>
    /// If <see langword="true"/>, the rule likely originated from a data annotation or fluent configuration.
    /// </remarks>
    bool HasValidator { get; }

    /// <summary>
    /// Evaluates whether the rule should be applied to the specified instance.
    /// </summary>
    /// <param name="targetInstance">The instance being validated.</param>
    /// <returns><see langword="true"/> if the rule should be applied; otherwise, <see langword="false"/>.</returns>
    bool ShouldValidate(object targetInstance);

    /// <summary>
    /// Asynchronously evaluates whether the rule should be applied to the specified instance.
    /// </summary>
    /// <param name="targetInstance">The instance being validated.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the rule should be applied; otherwise, <see langword="false"/>.</returns>
    Task<bool> ShouldValidateAsync(object targetInstance, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the <see cref="PropertyName"/>, if set, or the <see cref="MemberInfo.Name"/>
    /// value of the current <see cref="IValidationRule"/> instance.
    /// </summary>
    /// <returns>A string representing the property name of this rule.</returns>
    string GetPropertyName();
}
