using FluentAnnotationsValidator.Runtime.Validators;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Metadata;

public class Compare2Attribute(string otherProperty, ComparisonOperator @operator = ComparisonOperator.Equal) 
    : FluentValidationAttribute("The field '{0}' must satisfy the comparison with '{1}'.")
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var otherProp = instance.GetType().GetProperty(otherProperty, BindingFlags.Public | BindingFlags.Instance);

        if (otherProp is null)
            return new ValidationResult($"Property '{otherProperty}' not found on type '{instance.GetType().Name}'.");

        var otherValue = otherProp.GetValue(instance);

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

            return isValid ? ValidationResult.Success : GetFailedValidationResult(value, validationContext);
        }

        return new ValidationResult("Both values must implement IComparable.");
    }
}