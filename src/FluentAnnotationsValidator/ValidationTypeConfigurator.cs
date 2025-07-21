using System.Linq.Expressions;

namespace FluentAnnotationsValidator;

public class ValidationTypeConfigurator<T> : IValidationTypeConfigurator<T>
{
    private readonly ValidationConfigurator _parent;
    private readonly Type _type;

    public ValidationTypeConfigurator(ValidationConfigurator parent, Type type)
    {
        _parent = parent;
        _type = type;
    }

    public ValidationTypeConfigurator<T> When<TProp>(
        Expression<Func<T, TProp>> property,
        Func<T, bool> condition)
    {
        _parent.Register(opts => opts.AddCondition(property, condition));
        return this;
    }

    public ValidationTypeConfigurator<T> And<TProp>(
        Expression<Func<T, TProp>> property,
        Func<T, bool> condition) => When(property, condition);

    public ValidationTypeConfigurator<T> Except<TProp>(
        Expression<Func<T, TProp>> property)
    {
        _parent.Register(opts => opts.AddCondition(property, _ => false));
        return this;
    }

    public ValidationTypeConfigurator<T> AlwaysValidate<TProp>(
        Expression<Func<T, TProp>> property)
    {
        _parent.Register(opts => opts.AddCondition(property, _ => true));
        return this;
    }

    public ValidationTypeConfigurator<TNext> For<TNext>() => _parent.For<TNext>();

    public void Build() => _parent.Build();

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.When<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => When(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.And<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => And(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Except<TProp>(Expression<Func<T, TProp>> property)
        => Except(property);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.AlwaysValidate<TProp>(Expression<Func<T, TProp>> property)
        => When(property, _ => true);
}
