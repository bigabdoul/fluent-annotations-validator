using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Expression"/> objects.
/// </summary>
public static class ValidatorExpressionExtensions
{
    /// <summary>
    /// Extracts the <see cref="PropertyInfo"/> from a strongly typed property lambda expression.
    /// </summary>
    /// <typeparam name="T">The type containing the property.</typeparam>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="expression">A lambda like <c>x => x.Property</c></param>
    /// <returns>The <see cref="PropertyInfo"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the expression is not a simple member access.</exception>
    public static PropertyInfo GetPropertyInfo<T, TProp>(this Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression member && member.Member is PropertyInfo prop)
            return prop;

        throw new ArgumentException("Expression must be a simple property access like x => x.Property");
    }
}
