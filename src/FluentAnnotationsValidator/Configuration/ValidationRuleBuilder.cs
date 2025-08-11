using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Configuration;

public class ValidationRuleBuilder<T, TProp>(PendingRule<T> currentRule) : IValidationRuleBuilder<T, TProp>
{
    private Func<T, bool>? whenPredicate;

    //private PendingRule<T> CurrentRule => currentRule;

    internal List<ConditionalValidationRule> Rules { get; } = [];

    public IReadOnlyCollection<ConditionalValidationRule> GetRules() => Rules.AsReadOnly();

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
            attribute: new MustValidationAttribute<TProp>(predicate),
            composedPredicate);

        Rules.Add(rule);

        return this;
    }

    public IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        // Find the last When condition and negate it for the Otherwise clause.
        var lastCondition = GetLastConditionFromPendingRules()
            ?? throw new InvalidOperationException("Otherwise() must follow a When() call.");

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

    public IValidationRuleBuilder<T, TProp> AddRuleFromAttribute(ValidationAttribute attribute)
    {
        //currentRule.Attributes.Add(attribute);
        var member = currentRule.Member.GetMemberInfo();
        var rule = currentRule.CreateRuleFromPending(member,
            attribute,
            currentRule.Predicate);

        //rule.Message = attribute.FormatErrorMessage(member.Name);

        Rules.Add(rule);

        return this;
    }

    public IValidationRuleBuilder<T, TProp> WithMessage(string? message)
    {
        Rules.Last().Message = message;
        return this;
    }

    private Func<T, bool>? GetLastConditionFromPendingRules()
    {
        // This is a simplified way to get the condition; a more robust solution might store conditions separately.
        // Assuming the predicate for the last rule is the condition.
        return whenPredicate;
    }
}