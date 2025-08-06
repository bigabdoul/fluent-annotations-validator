using FluentAnnotationsValidator.Runtime.Validators;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Internals.Annotations;

public class CompareAttribute(string otherProperty, ComparisonOperator @operator) 
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

            if (isValid) return ValidationResult.Success;

            var message = MessageResolver?.ResolveMessage(
                validationContext.ObjectInstance.GetType(),
                validationContext.MemberName ?? validationContext.DisplayName ?? "field",
                this) ?? FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName ?? "field");

            return new ValidationResult($"{message}\nCompared to: {otherProperty}\nOperator: {@operator}\nLeft: {value}\nRight: {otherValue}");
        }

        return new ValidationResult("Both values must implement IComparable.");
    }
}