using System.Linq.Expressions;

namespace FluentAnnotationsValidator;

public class ValidationTypeConfigurator<T>(ValidationConfigurator parent) : IValidationTypeConfigurator<T>
{
    public ValidationTypeConfigurator<T> When<TProp>(
        Expression<Func<T, TProp>> property,
        Func<T, bool> condition)
    {
        parent.Register(opts => opts.AddCondition(property, condition));
        return this;
    }

    public ValidationTypeConfigurator<T> And<TProp>(
        Expression<Func<T, TProp>> property,
        Func<T, bool> condition) => When(property, condition);

    public ValidationTypeConfigurator<T> Except<TProp>(
        Expression<Func<T, TProp>> property)
    {
        parent.Register(opts => opts.AddCondition(property, _ => false));
        return this;
    }

    public ValidationTypeConfigurator<T> AlwaysValidate<TProp>(
        Expression<Func<T, TProp>> property)
    {
        parent.Register(opts => opts.AddCondition(property, _ => true));
        return this;
    }

    public ValidationTypeConfigurator<TNext> For<TNext>() => parent.For<TNext>();

    public void Build() => parent.Build();

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.When<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => When(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.And<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => And(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Except<TProp>(Expression<Func<T, TProp>> property)
        => Except(property);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.AlwaysValidate<TProp>(Expression<Func<T, TProp>> property)
        => When(property, _ => true);
}
