using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Abstractions;

public interface IValidationRuleBuilder
{
    Expression Member { get; }
    IReadOnlyCollection<ConditionalValidationRule> GetRules();
    int RemoveRules(Predicate<ConditionalValidationRule> predicate);
}


public interface IValidationRuleBuilder<T, TProp> : IValidationRuleBuilder
{
    IValidationRuleBuilder<T, TProp> When(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure);
    IValidationRuleBuilder<T, TProp> Must(Func<TProp, bool> predicate);
    IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure);
    IValidationRuleBuilder<T, TProp> WithMessage(string? message);
    IValidationRuleBuilder<T, TProp> AddRuleFromAttribute(ValidationAttribute attribute);
}
