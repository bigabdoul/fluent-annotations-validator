using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator;
using FluentLength = Internals.Annotations.LengthCountAttribute;

public static class ValidationTypeConfiguratorExtensions
{
    public static ValidationTypeConfigurator<T> Empty<T, TProp>(this ValidationTypeConfigurator<T> configurator, Expression<Func<T, TProp>> property)
    {
        configurator.AttachAttribute(property, new EmptyAttribute());
        return configurator;
    }

    /// <summary>
    /// Attach a new rule to a specific property; use for direct attachment.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    /// <param name="configurator"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static ValidationTypeConfigurator<T> NotEmpty<T, TProp>(this ValidationTypeConfigurator<T> configurator,
        Expression<Func<T, TProp>> property)
    {
        configurator.AttachAttribute(property, new NotEmptyAttribute());
        return configurator;
    }

    /// <summary>
    /// Attach rule to most recent .Rule(...) call; use for fluent chaining.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configurator"></param>
    /// <returns></returns>
    public static ValidationTypeConfigurator<T> NotEmpty<T>(this ValidationTypeConfigurator<T> configurator)
        => configurator.AttachAttribute(new NotEmptyAttribute());

    public static ValidationTypeConfigurator<T> MinimumLength<T>(this ValidationTypeConfigurator<T> configurator, int minimumLength)
        => configurator.Length(minimumLength, -1);

    //public static ValidationTypeConfigurator<T> MinimumLength<T, TProp>(this ValidationTypeConfigurator<T> configurator,
    //    Expression<Func<T, TProp>> property, int minimumLength)
    //{
    //    configurator.AttachAttribute(property, new FluentLength(minimumLength, -1));
    //    return configurator;
    //}

    public static ValidationTypeConfigurator<T> MaximumLength<T>(this ValidationTypeConfigurator<T> configurator, int maximumLength)
        => configurator.AttachAttribute(new FluentLength(0, maximumLength));

    public static ValidationTypeConfigurator<T> Length<T>(this ValidationTypeConfigurator<T> configurator, int min, int max)
        => configurator.AttachAttribute(new FluentLength(min, max));

    public static ValidationTypeConfigurator<T> Required<T>(this ValidationTypeConfigurator<T> configurator)
        => configurator.AttachAttribute(new RequiredAttribute());

    public static ValidationTypeConfigurator<T> Equal<T>(this ValidationTypeConfigurator<T> configurator, object? expected)
        => configurator.AttachAttribute(new EqualAttribute<object?>(expected));

    public static ValidationTypeConfigurator<T> Equal<T, TProp>(this ValidationTypeConfigurator<T> configurator,
        Expression<Func<T, TProp>> property, TProp expected, IEqualityComparer<TProp>? equalityComparer = null)
    {
        var attr = new EqualAttribute<TProp>(expected, equalityComparer);
        configurator.AttachAttribute(property, attr);
        return configurator;
    }

    public static ValidationTypeConfigurator<T> NotEqual<T>(this ValidationTypeConfigurator<T> configurator, object? disallowed)
        => configurator.AttachAttribute(new NotEqualAttribute<object?>(disallowed));

    public static ValidationTypeConfigurator<T> NotEqual<T, TProp>(this ValidationTypeConfigurator<T> configurator,
        Expression<Func<T, TProp>> property, TProp disallowed, IEqualityComparer<TProp>? equalityComparer = null)
    {
        var attr = new NotEqualAttribute<TProp>(disallowed, equalityComparer);
        configurator.AttachAttribute(property, attr);
        return configurator;
    }
}
