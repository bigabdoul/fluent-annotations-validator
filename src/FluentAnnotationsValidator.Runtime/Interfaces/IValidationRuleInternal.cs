using FluentAnnotationsValidator.Runtime.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Runtime.Interfaces;

/// <summary>
/// Defines a validation rule for a specific property.
/// </summary>
/// <typeparam name="T">The type of the instance being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property or field being validated.</typeparam>
internal interface IValidationRuleInternal<T, out TProperty> : IValidationRuleInternal
{
    /// <summary>
    /// Sets the display name for the property using a function.
    /// </summary>
    /// <param name="factory">The function for building the display name</param>
    void SetDisplayName(Func<ValidationContext, string> factory);

    /// <summary>
    /// Adds a validator to this rule.
    /// </summary>
    void AddValidator(ValidationAttribute validator);

    /// <summary>
    /// Adds an async validator to this rule.
    /// </summary>
    /// <param name="asyncValidator">The async property validator to invoke</param>
    /// <param name="fallback">A synchronous property validator to use as a fallback if executed synchronously. This parameter is optional. If omitted, the async validator will be called synchronously if needed.</param>
    void AddAsyncValidator(AsyncValidationAttribute asyncValidator, ValidationAttribute? fallback = null);

    /// <summary>
    /// The current rule component.
    /// </summary>
    ValidationAttribute Current { get; }
}

/// <summary>
/// Defines a rule associated with a property which can have multiple validators.
/// </summary>
internal interface IValidationRuleInternal
{
    /// <summary>
    /// The components in this rule.
    /// </summary>
    IEnumerable<ValidationAttribute> Components { get; }
    
    /// <summary>
    /// Returns the property name for the property being validated.
    /// Returns null if it is not a property being validated (eg a method call)
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Property associated with this rule.
    /// </summary>
    public MemberInfo Member { get; }

    /// <summary>
    /// Type of the property being validated
    /// </summary>
    public Type TypeToValidate { get; }

    /// <summary>
    /// Whether the rule has a condition defined.
    /// </summary>
    bool HasCondition { get; }

    /// <summary>
    /// Whether the rule has an async condition defined.
    /// </summary>
    bool HasAsyncCondition { get; }

    /// <summary>
    /// Expression that was used to create the rule.
    /// </summary>
    LambdaExpression Expression { get; }
}