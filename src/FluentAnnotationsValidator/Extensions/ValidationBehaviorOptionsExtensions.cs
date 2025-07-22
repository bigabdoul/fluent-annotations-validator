using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Runtime.Validators;
using System.Globalization;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ValidationBehaviorOptions"/> class.
/// </summary>
public static class ValidationBehaviorOptionsExtensions
{
    /// <summary>
    /// Registers a conditional validation rule for a specific model type and property.
    /// </summary>
    /// <typeparam name="TModel">The DTO or model type the condition applies to.</typeparam>
    /// <param name="options">The <see cref="ValidationBehaviorOptions"/> instance to configure.</param>
    /// <param name="propertyExpression">An expression identifying the property to which the condition applies.</param>
    /// <param name="predicate">A delegate that determines whether validation should be executed for the specified property.</param>
    /// <param name="message">An optional custom error message to override the default.</param>
    /// <param name="key">An optional error key used for message resolution or logging.</param>
    /// <param name="resourceKey">An optional resource key used for localized error messages.</param>
    /// <param name="resourceType"></param>
    /// <param name="culture">The culture-specific format provider to use.</param>
    /// <remarks>
    /// The condition will be stored and evaluated at runtime via <see cref="DataAnnotationsValidator{T}"/>. 
    /// Metadata such as <paramref name="message"/>, <paramref name="key"/>, and <paramref name="resourceKey"/> 
    /// are forwarded to the configured <see cref="IValidationMessageResolver"/>.
    /// </remarks>
    public static ConditionalValidationRule AddCondition<TModel>(this ValidationBehaviorOptions options,
        LambdaExpression propertyExpression,
        Func<TModel, bool> predicate,
        string? message = null,
        string? key = null,
        string? resourceKey = null,
        Type? resourceType = null,
        CultureInfo? culture = null,
        string? fallbackMessage = null,
        bool useConventionalKeys = true)
    {
        var rule = CreateValidationRule(propertyExpression, predicate, out var propertyName, 
            message, key, resourceKey, resourceType, culture, fallbackMessage, useConventionalKeys);
        options.Set(typeof(TModel), propertyName, rule);
        return rule;
    }

    internal static ConditionalValidationRule CreateValidationRule<TModel>(
        LambdaExpression propertyExpression,
        Func<TModel, bool> predicate,
        out string propertyName,
        string? message = null,
        string? key = null,
        string? resourceKey = null,
        Type? resourceType = null,
        CultureInfo? culture = null,
        string? fallbackMessage = null,
        bool useConventionalKeys = true)
    {
        propertyName = GetPropertyName(propertyExpression);
        var rule = new ConditionalValidationRule(model => predicate((TModel)model), 
            message, 
            key, 
            resourceKey, 
            resourceType, 
            culture,
            fallbackMessage,
            useConventionalKeys);
        return rule;
    }

    internal static string GetPropertyName(LambdaExpression propertyExpression)
    {
        var expr = (propertyExpression.Body as UnaryExpression)?.Operand;
        if (expr is MemberExpression me)
            return me.Member.Name;
        if (propertyExpression.Body is MemberExpression memberExpr)
            return memberExpr.Member.Name;
        throw new ArgumentException(
            $"{nameof(propertyExpression)}.Body is not a {nameof(MemberExpression)}.", nameof(propertyExpression));
    }
}
