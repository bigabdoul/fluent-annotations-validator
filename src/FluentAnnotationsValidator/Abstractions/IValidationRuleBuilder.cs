using FluentAnnotationsValidator.Configuration;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Abstractions;

public interface IValidationRuleBuilder
{
    IReadOnlyCollection<ConditionalValidationRule> GetRules();
}


public interface IValidationRuleBuilder<T, TProp> : IValidationRuleBuilder
{
    IValidationRuleBuilder<T, TProp> When(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure);
    IValidationRuleBuilder<T, TProp> Must(Func<TProp, bool> predicate);
    IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure);
    IValidationRuleBuilder<T, TProp> WithMessage(string? message);
    IValidationRuleBuilder<T, TProp> AddRuleFromAttribute(ValidationAttribute attribute);
}
