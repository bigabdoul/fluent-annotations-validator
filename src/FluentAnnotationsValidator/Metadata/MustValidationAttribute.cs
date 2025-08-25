using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Initializes a new instance of the <see cref="MustValidationAttribute{TProperty}"/> class.
/// </summary>
/// <typeparam name="TProperty">The type of the property the predicate operates on.</typeparam>
/// <param name="predicate">A function that performs the validation.</param>
/// <remarks>
/// This class represents an object that performs validation using the predicate specified in the constructor.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class MustValidationAttribute<TProperty>(Func<TProperty, bool> predicate) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || predicate((TProperty)value))
            return ValidationResult.Success;

        return new("The specified predicate doesn't satisfy the Must condition.");
    }
}
