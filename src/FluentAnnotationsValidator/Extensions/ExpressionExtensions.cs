using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Expression"/> objects.
/// </summary>
public static class ValidatorExpressionExtensions
{
    /// <summary>
    /// Extracts the <see cref="MemberInfo"/> from an (usually lambda) expression.
    /// </summary>
    /// <param name="expression">An lambda expression to extra the member info from, like <c>x => x.Property</c></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if the expression is not a simple member access.</exception>
    public static MemberInfo GetMemberInfo(this Expression expression)
    {
        if (expression is LambdaExpression { Body: MemberExpression expr } && expr.Member is MemberInfo info)
            return info;
        throw new ArgumentException("Expression must be a simple member access like x => x.Member");
    }

    public static object? GetMemberValue(this Expression expression, object instance)
    {
        return expression.GetMemberInfo() switch
        {
            PropertyInfo prop => prop.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            MethodInfo method when method.GetParameters().Length == 0 =>
                method.Invoke(instance, null),
            _ => null
        };
    }
}
