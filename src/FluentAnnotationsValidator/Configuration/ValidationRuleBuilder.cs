using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Implements a fluent, type-safe contract for building validation rules for a specific
/// property or field of a model.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TProp">The type of the property being validated.</typeparam>
/// <param name="currentRule">The <see cref="PendingRule{T}"/> for which the member is being configured.</param>
public class ValidationRuleBuilder<T, TProp>(PendingRule<T> currentRule) : IValidationRuleBuilder<T, TProp>
{
    private Func<T, bool>? whenPredicate;

    /// <summary>
    /// Gets the internally configured rules list.
    /// </summary>
    protected internal List<ConditionalValidationRule> Rules { get; } = [];

    /// <inheritdoc cref="IValidationRuleBuilder.Member"/>
    public Expression Member => currentRule.Member;

    /// <inheritdoc cref="IValidationRuleBuilder.GetRules"/>
    public IReadOnlyCollection<ConditionalValidationRule> GetRules() => Rules.AsReadOnly();

    /// <inheritdoc cref="IValidationRuleBuilder.RemoveRules(Predicate{ConditionalValidationRule})"/>
    public int RemoveRules(Predicate<ConditionalValidationRule> predicate) => Rules.RemoveAll(predicate);

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.When(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/>
    public IValidationRuleBuilder<T, TProp> When(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        // Create a new, temporary builder to hold the nested rules.
        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule);

        // Execute the action, which will populate the nested builder's Rules list.
        configure(nestedBuilder);

        var nestedBuilderRules = nestedBuilder.Rules;

        if (nestedBuilderRules.Count == 0)
            throw new InvalidOperationException("No rules configured.");

        bool ShouldApply(object value) => predicate((T)value);

        // For each rule collected by the nested builder, apply the 'When' condition.
        foreach (var nestedRule in nestedBuilderRules)
        {
            var originalPredicate = nestedRule.Predicate;

            // Compose a new predicate that checks the 'When' condition first,
            // and then the original nested rule's predicate.
            nestedRule.Predicate = model => predicate((T)model) && originalPredicate(model);

            nestedRule.SetShouldApply(ShouldApply);

            // Add the composed rule to the main rules list.
            Rules.Add(nestedRule);
        }

        // It's good practice to clear the temporary builder's rules.
        nestedBuilderRules.Clear();

        whenPredicate = predicate;

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.Must(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/>
    public IValidationRuleBuilder<T, TProp> Must(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure)
        => When(predicate, configure);

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.Must(Func{TProp, bool})"/>
    public IValidationRuleBuilder<T, TProp> Must(Func<TProp, bool> predicate)
    {
        var member = currentRule.Member;

        // Create a new predicate that takes the full model and extracts the member's value
        bool composedPredicate(T model)
        {
            var value = member.GetMemberValue(model!);
            // Cast the value to TProperty and pass it to the user's predicate
            return predicate((TProp)value!);
        }

        // Create and add a new pending rule with the composed predicate
        var rule = currentRule.CreateRuleFromPending(member.GetMemberInfo(),
            attribute: new MustAttribute<TProp>(predicate),
            composedPredicate);

        rule.SetShouldApply(_ => true);

        Rules.Add(rule);

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.Otherwise(Action{IValidationRuleBuilder{T, TProp}})"/>
    public IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        // Find the last When condition and negate it for the Otherwise clause.
        var lastCondition = GetLastConditionFromPendingRules()
            ?? throw new InvalidOperationException("Otherwise(...) must follow a call to " +
            "When(Func<T, bool>, Action<IValidationRuleBuilder<T, TProp>>) or " +
            "Must(Func<T, bool>, Action<IValidationRuleBuilder<T, TProp>>).");

        // Create a new, temporary configurator for the nested rules.
        var nestedConfigurator = new ValidationRuleBuilder<T, TProp>(currentRule);

        configure(nestedConfigurator);

        bool ShouldApply(object value) => !lastCondition((T)value);

        // Apply the negated condition to the nested rules.
        foreach (var nestedRule in nestedConfigurator.Rules)
        {
            var originalPredicate = nestedRule.Predicate;
            nestedRule.Predicate = model => ShouldApply(model) && originalPredicate(model);
            nestedRule.SetShouldApply(ShouldApply);

            Rules.Add(nestedRule);
        }

        nestedConfigurator.Rules.Clear();

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.AddRuleFromAttribute(ValidationAttribute)"/>
    public IValidationRuleBuilder<T, TProp> AddRuleFromAttribute(ValidationAttribute attribute)
    {
        //currentRule.Attributes.Add(attribute);
        var member = currentRule.Member.GetMemberInfo();
        var rule = currentRule.CreateRuleFromPending(member,
            attribute,
            currentRule.Predicate);

        Rules.Add(rule);

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.WithMessage(string?)"/>
    public IValidationRuleBuilder<T, TProp> WithMessage(string? message)
    {
        Rules.Last().Message = message;
        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.BeforeValidation(PreValidationValueProviderDelegate{T, TProp})"/>
    public IValidationRuleBuilder<T, TProp> BeforeValidation(PreValidationValueProviderDelegate<T, TProp> configure)
    {
        Rules.Last().ConfigureBeforeValidation = (instance, member, memberValue) => 
            configure.Invoke((T)instance, member, (TProp?)memberValue);
        return this;
    }

    private Func<T, bool>? GetLastConditionFromPendingRules()
    {
        // This is a simplified way to get the condition; a more robust solution might store conditions separately.
        // Assuming the predicate for the last rule is the condition.
        return whenPredicate;
    }
}