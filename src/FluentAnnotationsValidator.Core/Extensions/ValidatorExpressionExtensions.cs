using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Core.Extensions;

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
        if (expression is LambdaExpression lambda)
        {
            return lambda.Body is MemberExpression expr && expr.Member is MemberInfo info ? info : lambda.Body.Type;
        }
        throw new ArgumentException("Expression must be a simple member or type access like x => x.Member, or x => x");
    }

    /// <summary>
    /// Retrieves the value associated with the member contained in the <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to extract the <see cref="MemberInfo"/> from.</param>
    /// <param name="instance">The instance on which to retrieve the value associated with the extracted member info.</param>
    /// <returns>The value of the property, field, or method invocation result.</returns>
    /// <exception cref="NotSupportedException">The specified <paramref name="expression"/> is not supported.</exception>
    public static object? GetMemberValue(this Expression expression, object instance)
        => expression.GetMemberInfo().GetValue(instance);

    /// <summary>
    /// Retrieves the value of a member (property, field, or parameterless method) from an object instance.
    /// </summary>
    /// <remarks>
    /// This extension method uses pattern matching to dynamically get the value based on the type of the <see cref="MemberInfo"/>.
    /// It supports:
    /// <list type="bullet">
    /// <item><term><see cref="PropertyInfo"/></term><description>Gets the property's value using <see cref="PropertyInfo.GetValue(object)"/></description></item>
    /// <item><term><see cref="FieldInfo"/></term><description>Gets the field's value using <see cref="FieldInfo.GetValue(object)"/></description></item>
    /// <item><term><see cref="MethodInfo"/></term><description>Invokes a parameterless method and returns its result.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="member">The member whose value to get.</param>
    /// <param name="instance">The object instance from which to get the value.</param>
    /// <returns>The value of the member, or <see langword="null"/> if the member's value is null.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if the <paramref name="member"/> is not a supported type (PropertyInfo, FieldInfo, or a parameterless MethodInfo).
    /// </exception>
    public static object? GetValue(this MemberInfo member, object instance)
    {
        return member switch
        {
            PropertyInfo prop => prop.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            MethodInfo method when method.GetParameters().Length == 0 =>
                method.Invoke(instance, null),
            //ConstructorInfo => null,
            Type => instance,
            _ => throw new NotSupportedException("The specified expression is not supported.")
        };
    }

    /// <summary>
    /// Attempts to retrieve the value of a member (property, field, or parameterless method) from an object instance.
    /// </summary>
    /// <param name="member">The member whose value to get.</param>
    /// <param name="instance">The object instance from which to get the value.</param>
    /// <param name="value">Returns the value, if the operation is successful.</param>
    /// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetValue(this MemberInfo member, object instance, out object? value)
    {
        value = null;
        try
        {
            value = member.GetValue(instance);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    /// Sets the value of a member (property or field) on an object instance.
    /// </summary>
    /// <remarks>
    /// This extension method uses pattern matching to dynamically set the value based on the type of the <see cref="MemberInfo"/>.
    /// It supports:
    /// <list type="bullet">
    /// <item><term><see cref="PropertyInfo"/></term><description>Sets the property's value using <see cref="PropertyInfo.SetValue(object, object)"/></description></item>
    /// <item><term><see cref="FieldInfo"/></term><description>Sets the field's value using <see cref="FieldInfo.SetValue(object, object)"/></description></item>
    /// </list>
    /// This method does not support setting values on methods, constructors, or other member types.
    /// </remarks>
    /// <param name="member">The member whose value to set.</param>
    /// <param name="instance">The object instance on which to set the value.</param>
    /// <param name="value">The value to set the member to.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if the <paramref name="member"/> is not a supported type (PropertyInfo or FieldInfo).
    /// </exception>
    public static void SetValue(this MemberInfo member, object instance, object? value)
    {
        switch (member)
        {
            case PropertyInfo prop:
                prop.SetValue(instance, value);
                break;
            case FieldInfo field:
                field.SetValue(instance, value);
                break;
            default:
                throw new NotSupportedException($"The specified member type ({member.MemberType}) is not supported for setting a value.");
        }
    }

    /// <summary>
    /// Attempts to set the value of a member (property or field) on an object instance without throwing an exception.
    /// </summary>
    /// <remarks>
    /// This method calls the <see cref="SetValue(MemberInfo, object, object?)"/> extension method inside a try-catch block.
    /// It is a non-throwing alternative to <see cref="SetValue(MemberInfo, object, object?)"/> for scenarios where you need to gracefully
    /// handle cases where a value cannot be set, such as when a member is read-only, not a property or field, or the provided value is incompatible.
    /// </remarks>
    /// <param name="member">The member whose value to set. This should be a <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>.</param>
    /// <param name="instance">The object instance on which to set the value.</param>
    /// <param name="value">The value to set the member to.</param>
    /// <returns>
    /// <see langword="true"/> if the value was set successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TrySetValue(this MemberInfo member, object instance, object? value)
    {
        try
        {
            member.SetValue(instance, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified members are the same.
    /// </summary>
    /// <param name="sourceMemberInfo">The object to compare to <paramref name="targetMemberInfo"/>.</param>
    /// <param name="targetMemberInfo">The object to compare to <paramref name="sourceMemberInfo"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="MemberInfo.Name"/> property values of 
    /// <paramref name="sourceMemberInfo"/> and <paramref name="targetMemberInfo"/> are
    /// equal, and the <see cref="MemberInfo.ReflectedType"/> property values are assignable
    /// to each other in one or the other way; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool AreSameMembers(this MemberInfo sourceMemberInfo, MemberInfo targetMemberInfo)
    {
        if (ReferenceEquals(sourceMemberInfo, targetMemberInfo))
            return true;
        
        if (sourceMemberInfo is null || targetMemberInfo is null)
            return false;

        return sourceMemberInfo.MetadataToken == targetMemberInfo.MetadataToken &&
           sourceMemberInfo.Module == targetMemberInfo.Module;

        //return sourceMemberInfo.Name == targetMemberInfo.Name &&
        //    TypeUtils.IsAssignableFrom(sourceMemberInfo.DeclaringType, targetMemberInfo.ReflectedType);
    }

    /// <summary>
    /// Creates a unique, hashable key for a MemberInfo instance.
    /// This key is used to enable efficient lookups in a dictionary.
    /// </summary>
    /// <param name="member">The MemberInfo object to create a key for.</param>
    /// <returns>An object representing a unique key for the member.</returns>
    public static object AreSameMembersKey(this MemberInfo member)
    {
        // A tuple combining metadata token and module handle provides a unique, hashable key.
        // This is a robust way to identify members, even across different assemblies.
        return (member.MetadataToken, member.Module.ModuleHandle);
    }

    /// <summary>
    /// Checks if the expression is a parameter expression
    /// </summary>
    /// <param name="expression">The lambda expression to check.</param>
    /// <returns><see langword="true"/> if the specified expression is a lambda; otherwise, <see langword="false"/>.</returns>
    public static bool IsParameterExpression(this LambdaExpression expression)
    {
        return expression.Body.NodeType == ExpressionType.Parameter;
    }
}
