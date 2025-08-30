using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NotEqualAttribute(object? unexpectedValue, IEqualityComparer<object?>? equalityComparer = null,
    string errorMessage = "The field '{0}' must not equal the unexpected value.")
    : FluentValidationAttribute(errorMessage)
{
    private readonly IEqualityComparer<object?> _comparer = equalityComparer ?? EqualityComparer<object?>.Default;
    public object? Unexpected => unexpectedValue;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (!_comparer.Equals(value, unexpectedValue))
            return ValidationResult.Success;

        var message = MessageResolver?.ResolveMessage(
            validationContext.ObjectInstance.GetType(),
            validationContext.MemberName ?? validationContext.DisplayName ?? "field",
            this) ?? FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName ?? "field");

        return new ValidationResult($"{message}\nUnexpected: {unexpectedValue}\nActual: {value}");
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NotEqualAttribute<TProperty>(TProperty unexpectedValue, IEqualityComparer<TProperty>? equalityComparer = null) 
    : NotEqualAttribute(unexpectedValue)
{
    private readonly IEqualityComparer<TProperty> _comparer = equalityComparer ?? EqualityComparer<TProperty>.Default;

    public new TProperty? Unexpected => (TProperty?)base.Unexpected;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is TProperty typedValue)
        {
            if (!_comparer.Equals(typedValue, unexpectedValue))
                return ValidationResult.Success;

            var message = MessageResolver?.ResolveMessage(
                validationContext.ObjectInstance.GetType(),
                validationContext.MemberName ?? validationContext.DisplayName ?? "field",
                this) ?? FormatErrorMessage(validationContext.DisplayName ?? validationContext.MemberName ?? "field");

            return new ValidationResult($"{message}\nUnexpected: {unexpectedValue}\nActual: {typedValue}");
        }

        return new ValidationResult($"Type mismatch.\nExpected: {typeof(TProperty).Name}\nActual: {value.GetType().Name}");
    }
}