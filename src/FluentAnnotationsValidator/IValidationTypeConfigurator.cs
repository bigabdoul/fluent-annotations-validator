using System.Linq.Expressions;

namespace FluentAnnotationsValidator;

public interface IValidationTypeConfigurator<T>
{
    IValidationTypeConfigurator<T> When<TProp>(
        Expression<Func<T, TProp>> property,
        Func<T, bool> condition);

    IValidationTypeConfigurator<T> And<TProp>(
        Expression<Func<T, TProp>> property,
        Func<T, bool> condition);

    IValidationTypeConfigurator<T> Except<TProp>(
        Expression<Func<T, TProp>> property);

    IValidationTypeConfigurator<T> AlwaysValidate<TProp>(
        Expression<Func<T, TProp>> property);
}
