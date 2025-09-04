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
/// property or field of a model.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TProp">The type of the property being validated.</typeparam>
public interface IValidationRuleBuilder<T, TProp> : IValidationRuleBuilder
{
    /// <summary>
    /// Defines a rule for each item in a nested collection.
    /// </summary>
    /// <remarks>
    /// This method is designed to validate nested collections within a model. For top-level
    /// collections, use the 
    /// <see cref="IValidationTypeConfigurator{T}.RuleForEach{TElement}(Expression{Func{T, IEnumerable{TElement}}})"/>
    /// method on the main configurator.
    /// </remarks>
    /// <typeparam name="TElement">The type of the elements in the nested collection.</typeparam>
    /// <param name="member">The expression that contains the nested collection property.</param>
    /// <returns>
    /// A new instance of a class that implements the <see cref="IValidationRuleBuilder{T, TProp}"/> interface 
    /// for the specified type <typeparamref name="T"/>, and element type <typeparamref name="TElement"/>.
    /// </returns>
    IValidationRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<TProp, IEnumerable<TElement>>> member);

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
    /// Adds a conditional group of rules that will only be executed if the specified asynchronous predicate is true.
    /// </summary>
    /// <param name="predicate">An asynchronous predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    IValidationRuleBuilder<T, TProp> WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action<IValidationRuleBuilder<T, TProp>> configure);

    /// <summary>
    /// Alias for <see cref="When(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/>
    /// to make the intent clearer for complex validation logics.
    /// </summary>
    /// <param name="predicate">A function that returns <see langword="true"/> to enable the rules.</param>
    /// <param name="configure">An action to configure the validation rules to apply when the condition is met.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> Must(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure);

    /// <summary>
    /// Specifies a custom validation rule for the current member using a synchronous delegate.
    /// The rule passes if the delegate returns <see langword="true"/>.
    /// </summary>
    /// <param name="predicate">A delegate that contains the custom validation logic.</param>
    /// <returns>The current rule builder instance for chaining.</returns>
    IValidationRuleBuilder<T, TProp> Must(Func<TProp, bool> predicate);

    /// <summary>
    /// Specifies a custom asynchronous validation rule for the current member.
    /// The rule passes if the asynchronous delegate returns <see langword="true"/>.
    /// </summary>
    /// <param name="predicate">An asynchronous delegate that contains the custom validation logic.</param>
    /// <returns>The current rule builder instance for chaining.</returns>
    IValidationRuleBuilder<T, TProp> MustAsync(Func<TProp?, CancellationToken, Task<bool>> predicate);

    /// <summary>
    /// Applies a conditional predicate to all subsequent rules in the chain.
    /// Rules added within the <paramref name="configure"/> action will only be executed
    /// if the last `When` or `Otherwise` predicate is false.
    /// </summary>
    /// <param name="configure">An action to configure the validation rules to apply when the condition is met.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure);

    /// <summary>
    /// Provides a way to define asynchronous rules that will be applied if the preceding WhenAsync condition is not met.
    /// </summary>
    /// <param name="configure">A function to configure the rule that will be executed.</param>
    /// <returns>The current rule builder instance for chaining.</returns>
    IValidationRuleBuilder<T, TProp> OtherwiseAsync(Func<IValidationRuleBuilder<T, TProp>, Task> configure);

    /// <summary>
    /// Sets a custom error message for the most recently added rule.
    /// </summary>
    /// <param name="message">The custom message string. Can contain format placeholders like `{0}`.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> WithMessage(string? message);

    /// <summary>
    /// Sets a custom error message for the most recently added via delegate.
    /// </summary>
    /// <param name="messageResolver">A delegate to set the custom message string.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> WithMessage(Func<T, string> messageResolver);

    /// <summary>
    /// Specifies the property name to use in the generated validation message.
    /// </summary>
    /// <param name="propertyName">The name of the property to be used in the message.</param>
    /// <returns>The current rule builder instance for chaining.</returns>
    IValidationRuleBuilder<T, TProp> OverridePropertyName(string propertyName);

    /// <summary>
    /// Adds a new validation rule to the builder based on the logic of a <see cref="ValidationAttribute"/>.
    /// </summary>
    /// <param name="attribute">The <see cref="ValidationAttribute"/> to use for the validation rule.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> SetAttributeValidator(ValidationAttribute attribute);

    /// <summary>
    /// Gives a rule a chance to gather the correct value for the specified member before validation occurs.
    /// </summary>
    /// <param name="configure">A delegate that configures the instance and member on pre-validation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    IValidationRuleBuilder<T, TProp> BeforeValidation(PreValidationValueProviderDelegate<T, TProp> configure);
}