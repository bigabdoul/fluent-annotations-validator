using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines the non-generic contract for a validation rule builder.
/// </summary>
public interface IValidationRuleBuilder
{
    /// <summary>
    /// Gets the expression representing the member (property or field) to which the validation rules apply.
    /// </summary>
    Expression Member { get; }

    /// <summary>
    /// Gets a read-only collection of the conditional validation rules that have been added to this builder.
    /// </summary>
    /// <returns>A collection of <see cref="ConditionalValidationRule"/> instances.</returns>
    IReadOnlyCollection<ConditionalValidationRule> GetRules();

    /// <summary>
    /// Removes rules from the builder that match the specified predicate.
    /// </summary>
    /// <param name="predicate">A function that defines the conditions of the rules to remove.</param>
    /// <returns>The number of rules that were removed from the builder.</returns>
    int RemoveRules(Predicate<ConditionalValidationRule> predicate);
}

/// <summary>
/// Defines a fluent, type-safe contract for building validation rules for a specific
/// property of a model.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TProp">The type of the property being validated.</typeparam>
public interface IValidationRuleBuilder<T, TProp> : IValidationRuleBuilder
{
    /// <summary>
    /// Applies a conditional predicate to all subsequent rules in the chain.
    /// Rules added within the <paramref name="configure"/> action will only be executed
    /// if the <paramref name="predicate"/> is true.
    /// </summary>
    /// <param name="predicate">A function that returns <see langword="true"/> to enable the rules.</param>
    /// <param name="configure">An action to configure the validation rules to apply when the condition is met.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> When(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure);

    /// <summary>
    /// Adds a custom rule that validates the property's value based on a predicate function.
    /// </summary>
    /// <param name="predicate">A function that returns <see langword="true"/> if the property's value is valid.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> Must(Func<TProp, bool> predicate);

    /// <summary>
    /// Applies a conditional predicate to all subsequent rules in the chain.
    /// Rules added within the <paramref name="configure"/> action will only be executed
    /// if the last `When` or `Otherwise` predicate is false.
    /// </summary>
    /// <param name="configure">An action to configure the validation rules to apply when the condition is met.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure);

    /// <summary>
    /// Sets a custom error message for the most recently added rule.
    /// </summary>
    /// <param name="message">The custom message string. Can contain format placeholders like `{0}`.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> WithMessage(string? message);

    /// <summary>
    /// Adds a new validation rule to the builder based on the logic of a <see cref="ValidationAttribute"/>.
    /// </summary>
    /// <param name="attribute">The <see cref="ValidationAttribute"/> to use for the validation rule.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> AddRuleFromAttribute(ValidationAttribute attribute);
}