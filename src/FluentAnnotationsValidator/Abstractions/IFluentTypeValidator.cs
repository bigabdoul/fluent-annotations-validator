using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Provides a fluent interface for configuring conditional validation rules on a specific model type.
/// Supports chaining validation logic, metadata overrides, and transitions to other model configurators.
/// </summary>
/// <typeparam name="T">The model type being configured.</typeparam>
public interface IFluentTypeValidator<T>
{
    /// <summary>
    /// Transitions to configuring a different model type.
    /// </summary>
    /// <typeparam name="TNext">The next model type to configure.</typeparam>
    /// <returns>A configurator for the specified model type.</returns>
    IFluentTypeValidator<TNext> For<TNext>();

    /// <summary>
    /// Creates a preemptive rule that overrides all previously registered rules for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member.</typeparam>
    /// <param name="member">The expression that contains the property, field, or method info.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member);

    /// <summary>
    /// Creates a preemptive rule that optionally overrides all previously registered rules for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member.</typeparam>
    /// <param name="member">The expression that contains the property, field, or method info.</param>
    /// <param name="behavior">
    /// A value that indicates whether to replace rules for the specified <paramref name="member"/>.
    /// </param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member, RuleDefinitionBehavior behavior);

    /// <summary>
    /// Creates a preemptive, conditionally executed rule that overrides 
    /// all previously registered rules for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member.</typeparam>
    /// <param name="member">The expression that contains the property, field, or method info.</param>
    /// <param name="must">A function that performs the validation.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must);

    /// <summary>
    /// Creates a preemptive, conditionally executed rule that optionally 
    /// overrides all previously registered rules for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member.</typeparam>
    /// <param name="member">The expression that contains the property, field, or method info.</param>
    /// <param name="must">A function that performs the validation.</param>
    /// <param name="behavior">
    /// A value that indicates whether to replace rules for the specified <paramref name="member"/>.
    /// </param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must, RuleDefinitionBehavior behavior);

    /// <summary>
    /// Creates a non-preemptive rule, that is a rule that preserves 
    /// all previously registered rules, for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member.</typeparam>
    /// <param name="member">The expression that contains the property, field, or method info.</param>
    /// <returns>
    /// A new instance of a class that implements the <see cref="IValidationRuleBuilder{T, TProp}"/> interface 
    /// for the specified type <typeparamref name="T"/>, and member type <typeparamref name="TMember"/>.
    /// </returns>
    IValidationRuleBuilder<T, TMember> RuleFor<TMember>(Expression<Func<T, TMember>> member);

    /*
    /// <summary>
    /// Defines a validation rule for each item in a collection property.
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the collection.</typeparam>
    /// <param name="expression">The expression that contains the collection property.</param>
    /// <returns>
    /// A new instance of a class that implements the <see cref="IValidationRuleBuilder{T, TProp}"/> interface 
    /// for the specified type <typeparamref name="T"/>, and element type <typeparamref name="TElement"/>.
    /// </returns>
    ICollectionRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression);
    */

    /// <summary>
    /// Removes all validation rules currently registered for the specified member.
    /// This includes both statically defined (attribute-based) rules and dynamically added fluent rules.
    /// </summary>
    /// <typeparam name="TMember">The type of the member (property or field) to remove rules for.</typeparam>
    /// <param name="memberExpression">An expression identifying the member for which to remove all rules.</param>
    /// <returns>The current configurator instance for further chaining.</returns>
    IFluentTypeValidator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> memberExpression);

    /// <summary>
    /// Removes all validation rules of a specific attribute type for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member (property or field) to remove rules for.</typeparam>
    /// <typeparam name="TAttribute">The exact type of the <see cref="ValidationAttribute"/> to remove.</typeparam>
    /// <param name="memberExpression">An expression identifying the member from which to remove the specific attribute's rules.</param>
    /// <returns>The current configurator instance for further chaining.</returns>
    IFluentTypeValidator<T> RemoveRulesFor<TMember, TAttribute>(Expression<Func<T, TMember>> memberExpression) where TAttribute : ValidationAttribute;

    /// <summary>
    /// Removes all validation rules of a specific attribute type (provided as a <see cref="Type"/> object)
    /// for the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member (property or field) to remove rules for.</typeparam>
    /// <param name="memberExpression">An expression identifying the member from which to remove the specific attribute's rules.</param>
    /// <param name="attributeType">The <see cref="Type"/> of the <see cref="ValidationAttribute"/> to remove.</param>
    /// <returns>The current configurator instance for further chaining.</returns>
    IFluentTypeValidator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> memberExpression, Type attributeType);

    /// <summary>
    /// Removes all validation rules for all members of the configured type <typeparamref name="T"/>,
    /// except for those rules associated with the specified member.
    /// </summary>
    /// <typeparam name="TMember">The type of the member (property or field) whose rules should be preserved.</typeparam>
    /// <param name="memberExpression">An expression identifying the single member for which to keep rules.</param>
    /// <returns>The current configurator instance for further chaining.</returns>
    IFluentTypeValidator<T> RemoveRulesExceptFor<TMember>(Expression<Func<T, TMember>> memberExpression);

    /// <summary>
    /// Clears all validation rules currently registered for the configured type 
    /// <typeparamref name="T"/>. This operation includes all pending fluent rules 
    /// and all rules registered in the <see cref="ValidationRuleGroupRegistry"/>.
    /// </summary>
    /// <returns>The current configurator instance for further chaining.</returns>
    IFluentTypeValidator<T> ClearRules();

    /// <summary>
    /// Removes all pending rules for the specified member.
    /// </summary>
    /// <param name="memberInfo">An object containing information about the member to remove rules for.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> RemovePendingRules(MemberInfo memberInfo);

    /// <summary>
    /// Removes all pending rules for the specified member and validation attribute type.
    /// </summary>
    /// <param name="memberInfo">An object containing information about the member to remove rules for.</param>
    /// <param name="validationAttributeType">The type of validation to remove rules for.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> RemovePendingRules(MemberInfo memberInfo, Type validationAttributeType);

    /// <summary>
    /// Sets the default resource type for localization lookups in this configuration chain.
    /// </summary>
    /// <typeparam name="TResource">The type parameter of the validation resource type to use.</typeparam>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WithValidationResource<TResource>();

    /// <summary>
    /// Sets the default resource type for localization lookups in this configuration chain.
    /// </summary>
    /// <param name="resourceType">The validation resource type to use. Can be null.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WithValidationResource(Type? resourceType);

    /// <summary>
    /// Sets the culture used during error message resolution.
    /// </summary>
    /// <param name="culture">The culture information to set.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WithCulture(CultureInfo culture);

    /// <summary>
    /// Adds a conditional validation rule that applies to the entire model instance.
    /// All subsequent validation rules will only be executed if the specified condition is met.
    /// </summary>
    /// <param name="condition">A predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> When(Predicate<T> condition);

    /// <summary>
    /// Adds a conditional validation rule for a given property.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="property">An expression identifying the target property.</param>
    /// <param name="condition">A predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> When<TProp>(Expression<Func<T, TProp>> property, Predicate<T> condition);

    /// <summary>
    /// Adds a conditional validation rule for a given property that supports asynchronous operations.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WhenAsync(Func<T, CancellationToken, Task<bool>> condition);

    /// <summary>
    /// Adds a conditional validation rule for a given property that supports asynchronous operations.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="property">An expression identifying the target property.</param>
    /// <param name="condition">An asynchronous predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WhenAsync<TProp>(Expression<Func<T, TProp>> property, Func<T, CancellationToken, Task<bool>> condition);

    /// <summary>
    /// Adds an additional conditional rule for another property. Alias for <see cref="When{TProp}"/>.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="property">An expression identifying the target property.</param>
    /// <param name="condition">A predicate that determines whether validation should execute.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> And<TProp>(Expression<Func<T, TProp>> property, Predicate<T> condition);

    /// <summary>
    /// Excludes the specified property from validation entirely.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to exclude.</typeparam>
    /// <param name="property">An expression identifying the property to skip.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> Except<TProp>(Expression<Func<T, TProp>> property);

    /// <summary>
    /// Forces unconditional validation for the specified property.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to validate.</typeparam>
    /// <param name="property">An expression identifying the property to validate.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> AlwaysValidate<TProp>(Expression<Func<T, TProp>> property);

    /// <summary>
    /// Attaches a custom error message to the current validation condition.
    /// </summary>
    /// <param name="message">The text to display when the condition fails.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WithMessage(string message);

    /// <summary>
    /// Attaches an explicit error key to the current validation condition.
    /// </summary>
    /// <param name="key">The failure key used by the message resolver or diagnostics.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> WithKey(string key);

    /// <summary>
    /// Specifies a resource key for localized error messages tied to the current condition.
    /// </summary>
    /// <param name="resourceKey">The key used to lookup localized text from a resource provider.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> Localized(string resourceKey);

    /// <summary>
    /// Explicitly disables "Property_Attribute" fallback lookup - for projects relying solely on .WithKey(...).
    /// </summary>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> DisableConventionalKeys();

    /// <summary>
    /// Configures whether configuration enforcement is enabled for the model type.
    /// </summary>
    /// <remarks>
    /// When configuration enforcement is disabled, the validation engine will not apply
    /// any rules that were configured via the fluent API for this model type, effectively
    /// reverting to only using rules defined by <see cref="ValidationAttribute"/>s.
    /// </remarks>
    /// <param name="disableConfigurationEnforcement">
    /// A boolean value. Set to <see langword="true"/> to disable configuration enforcement, or <see langword="false"/> to enable it.
    /// </param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> DisableConfigurationEnforcement(bool disableConfigurationEnforcement);

    /// <summary>
    /// Specifies a message to fall back to if .Localized(...) lookup fails - avoids silent runtime fallback.
    /// </summary>
    /// <param name="fallbackMessage">The fallback message to use.</param>
    /// <returns>The current configurator for further chaining.</returns>
    IFluentTypeValidator<T> UseFallbackMessage(string fallbackMessage);

    /// <summary>
    /// Configures a pre-validation value provider for the current type.
    /// </summary>
    /// <remarks>
    /// This method allows you to provide a custom value provider delegate that will be invoked just 
    /// before any validation rules are executed for properties of this type. It is useful for scenarios 
    /// where you need to perform data preparation (like auto-generated values/identifiers) or value 
    /// retrieval (e.g., from an external source) before validation.
    /// </remarks>
    /// <param name="configure">A delegate that provides a new value for the instance before validation.</param>
    /// <returns>A reference to the current configurator instance for method chaining.</returns>
    IFluentTypeValidator<T> BeforeValidation(PreValidationValueProviderDelegate<T> configure);

    /// <summary>
    /// Conditionally attaches a validation attribute to the current validation configuration.
    /// </summary>
    /// <param name="attribute">The validation attribute instance to attach.</param>
    /// <param name="when">A function that determines when the rule should be applied.</param>
    /// <returns>The current <see cref="IFluentTypeValidator{T}"/> instance for fluent chaining.</returns>
    IFluentTypeValidator<T> AttachAttribute(ValidationAttribute attribute, Predicate<T>? when = null);

    /// <summary>
    /// Finalizes the configuration of validation rules for the type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>A read-only list of <see cref="IValidationRule"/> objects that were built.</returns>
    IReadOnlyList<IValidationRule> Build();

    /// <summary>
    /// Clears the rules from the last <see cref="Build"/> generation.
    /// </summary>
    void DiscardRulesFromLastBuild();
}
