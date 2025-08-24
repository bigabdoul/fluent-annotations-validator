using FluentAnnotationsValidator.Results;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    /// <summary>
    /// Retrieves the value associated with the member contained in the <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to extract the <see cref="MemberInfo"/> from.</param>
    /// <param name="instance">The instance on which to retrieve the value associated with the extracted member info.</param>
    /// <returns>The value of the property, field, or method invocation result.</returns>
    /// <exception cref="NotSupportedException">The specified <paramref name="expression"/> is not supported.</exception>
    public static object? GetMemberValue(this Expression expression, object instance)
    {
        return expression.GetMemberInfo() switch
        {
            PropertyInfo prop => prop.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            MethodInfo method when method.GetParameters().Length == 0 =>
                method.Invoke(instance, null),
            //ConstructorInfo => null,
            _ => throw new NotSupportedException("The specified expression is not supported.")
        };
    }

    /// <summary>
    /// Determines whether the specified members are the same.
    /// </summary>
    /// <param name="sourceMemberInfo">The object to compare to <paramref name="targetMemberInfo"/>.</param>
    /// <param name="targetMemberInfo">The object to compare to <paramref name="sourceMemberInfo"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="MemberInfo.Name"/> property values of 
    /// <paramref name="sourceMemberInfo"/> and <paramref name="targetMemberInfo"/> are
    /// equal, and the <see cref="MemberInfo.DeclaringType"/> property values are assignable
    /// to each other in one or the other way; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool AreSameMembers(this MemberInfo sourceMemberInfo, MemberInfo targetMemberInfo)
    {
        return sourceMemberInfo.Name == targetMemberInfo.Name &&
        (
            true == sourceMemberInfo.DeclaringType?.IsAssignableFrom(targetMemberInfo.DeclaringType) ||
            true == sourceMemberInfo.DeclaringType?.IsAssignableTo(targetMemberInfo.DeclaringType)
        );
    }

    internal static bool IsSameRule(this (MemberInfo, ValidationAttribute?) source, (MemberInfo, ValidationAttribute?) target)
    {
        var (member1, attr1) = source;
        var (member2, attr2) = target;
        return member1.AreSameMembers(member2) && attr1?.GetType() == attr2?.GetType();
    }
}
