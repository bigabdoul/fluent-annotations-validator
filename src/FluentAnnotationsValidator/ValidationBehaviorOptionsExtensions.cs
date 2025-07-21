using System.Linq.Expressions;

namespace FluentAnnotationsValidator;

/// <summary>
/// Provides extension methods for the <see cref="ValidationBehaviorOptions"/> class.
/// </summary>
public static class ValidationBehaviorOptionsExtensions
{
    /// <summary>
    /// Registers a conditional validation predicate for a given property.
    /// </summary>
    /// <typeparam name="TModel">The DTO type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="options">The options instance being configured.</param>
    /// <param name="propertyExpression">The property selector (e.g. x => x.Email).</param>
    /// <param name="condition">Predicate to control whether validation should run for the selected property.</param>
    public static void AddCondition<TModel, TProperty>(
        this ValidationBehaviorOptions options,
        Expression<Func<TModel, TProperty>> propertyExpression,
        Func<TModel, bool> condition,
        string? message = null,
        string? key = null,
        string? resourceKey = null)
    {
        var propertyName = GetPropertyName(propertyExpression);
        var rule = new ConditionalValidationRule(model => condition((TModel)model), message, key, resourceKey);
        options.Set(typeof(TModel), propertyName, rule);
    }

    private static string GetPropertyName<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpr)
            return memberExpr.Member.Name;

        throw new ArgumentException("Expression must be a property access", nameof(expression));
    }
}
