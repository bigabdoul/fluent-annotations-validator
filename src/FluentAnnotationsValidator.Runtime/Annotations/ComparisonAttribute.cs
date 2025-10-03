using FluentAnnotationsValidator.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Runtime.Annotations;

using Core.Extensions;
using Core.Interfaces;
using Runtime.Extensions;

/// <summary>
/// Specifies that a field or property's value must satisfy a specific comparison
/// with another property's value on the same object.
/// </summary>
/// <remarks>
/// This attribute provides a more flexible alternative to the built-in <see cref="System.ComponentModel.DataAnnotations.CompareAttribute"/>
/// by allowing for various comparison operators (e.g., greater than, less than, not equal to).
/// It requires that both the validated property and the compared property implement <see cref="IComparable"/>.
/// </remarks>
/// <param name="otherProperty">The name of the property to compare with.</param>
/// <param name="operator">
/// The comparison operator to use. The default is <see cref="ComparisonOperator.Equal"/>.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ComparisonAttribute(string otherProperty, ComparisonOperator @operator = ComparisonOperator.Equal)
    : CompareAttribute(GetErrorMessageFormat(@operator))
{
    /// <summary>
    /// Gets the default error message format string for a given comparison operator.
    /// </summary>
    /// <param name="comparison">The comparison operator.</param>
    /// <returns>A formatted error message string.</returns>
    protected static string GetErrorMessageFormat(ComparisonOperator comparison)
        => "The field '{0}' must satisfy the " + comparison.ToString() + " comparison with '{1}'.";

    /// <summary>
    /// Gets or sets an optional message resolver to provide localized error messages.
    /// </summary>
    public IValidationMessageResolver? MessageResolver { get; set; }

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherMember = validationContext.ObjectType.GetMember(otherProperty).FirstOrDefault();

        if (otherMember is null)
            return new ValidationResult($"Member '{otherProperty}' not found on type '{validationContext.ObjectType.Name}'.");

        var otherValue = otherMember.GetValue(validationContext.ObjectInstance);

        if (value is IComparable left && otherValue is IComparable right)
        {
            var comparison = left.CompareTo(right);
            var isValid = @operator switch
            {
                ComparisonOperator.Equal => comparison == 0,
                ComparisonOperator.NotEqual => comparison != 0,
                ComparisonOperator.GreaterThan => comparison > 0,
                ComparisonOperator.LessThan => comparison < 0,
                ComparisonOperator.GreaterThanOrEqual => comparison >= 0,
                ComparisonOperator.LessThanOrEqual => comparison <= 0,
                _ => throw new InvalidOperationException("Unsupported comparison operator.")
            };

            return isValid ? ValidationResult.Success : this.GetFailedValidationResult(validationContext, MessageResolver);
        }

        return new ValidationResult("Both values must implement IComparable.");
    }

    /// <inheritdoc/>
    public override string FormatErrorMessage(string name)
        => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, otherProperty);
}