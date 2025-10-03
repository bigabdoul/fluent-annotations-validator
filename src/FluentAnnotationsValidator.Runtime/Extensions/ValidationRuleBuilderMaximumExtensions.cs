using System.ComponentModel.DataAnnotations;

#pragma warning disable IDE0130 // Namespace "FluentAnnotationsValidator" does not match folder structure, expected "FluentAnnotationsValidator.Runtime.Extensions"

namespace FluentAnnotationsValidator;

#pragma warning restore IDE0130

using Annotations;
using Runtime.Interfaces;

/// <summary>
/// Provides extension methods for applying maximum value validation rules using <see cref="IValidationRuleBuilder{T, TProp}"/>
/// </summary>
/// <remarks>
/// These methods enable fluent composition of maximum constraints across numeric, enum, and comparable types.
/// </remarks>
public static class ValidationRuleBuilderMaximumExtensions
{ 
    #region MaximumAttribute

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, byte value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, short value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, int value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, long value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, Int128 value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, IntPtr value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, float value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, double value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, decimal value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, TProp value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value!)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    #endregion

    #region unsigned overloads

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, ushort value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, uint value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, ulong value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, UIntPtr value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    /// <summary>
    /// Specifies that the value of the property must be less than or equal to the given maximum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TProp">The type of the property being validated.</typeparam>
    /// <param name="builder">The validation rule builder.</param>
    /// <param name="value">The maximum allowable value.</param>
    /// <param name="isExclusive">Specifies whether validation should fail for values that are equal to <paramref name="value"/>.</param>
    /// <param name="parseLimitsInInvariantCulture">Indicates whether <see cref="RangeAttribute.Maximum"/> string value is parsed using invariant culture instead of the current culture.</param>
    /// <param name="convertValueInInvariantCulture">Indicates whether value conversions for <see cref="RangeAttribute.OperandType"/> use invariant culture instead of the current culture.</param>
    /// <returns>The updated validation rule builder.</returns>
    public static IValidationRuleBuilder<T, TProp> Maximum<T, TProp>(this IValidationRuleBuilder<T, TProp> builder, UInt128 value,
        bool isExclusive = false, bool parseLimitsInInvariantCulture = false, bool convertValueInInvariantCulture = false)
        => builder.SetValidator(new MaximumAttribute(value)
        {
            MaximumIsExclusive = isExclusive,
            ParseLimitsInInvariantCulture = parseLimitsInInvariantCulture,
            ConvertValueInInvariantCulture = convertValueInInvariantCulture
        });

    #endregion
}