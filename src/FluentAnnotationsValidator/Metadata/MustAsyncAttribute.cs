using FluentAnnotationsValidator.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// A validation attribute that enables asynchronous validation using a custom predicate.
/// </summary>
/// <remarks>
/// This attribute demonstrates how to implement <see cref="IAsyncValidationAttribute"/>.
/// It relies on an asynchronous predicate to perform its validation logic, ensuring that
/// the validation process remains non-blocking.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="MustAsyncAttribute"/> class.
/// </remarks>
/// <param name="predicate">The asynchronous function that performs the validation. It must return <c>true</c> for a valid state.</param>
public sealed class MustAsyncAttribute(Func<object?, CancellationToken, Task<bool>> predicate) : ValidationAttribute, IAsyncValidationAttribute
{
    /// <summary>
    /// This method is part of the synchronous <see cref="ValidationAttribute"/> contract.
    /// To ensure no deadlocks, this attribute is designed to be used with an asynchronous
    /// validation pipeline that calls <see cref="ValidateAsync"/>. This method should
    /// not be called directly.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context for the validation.</param>
    /// <returns>A validation result.</returns>
    [DoesNotReturn]
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // This attribute is designed to work with an asynchronous pipeline.
        // Returning a success result to avoid a silent failure but a robust
        // implementation should likely throw.
        //return ValidationResult.Success;
        throw new InvalidOperationException("This attribute is designed to work with an asynchronous pipeline.");
    }

    /// <summary>
    /// Asynchronously validates the specified value with the given validation context.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information for the validation.</param>
    /// <param name="cancellationToken">An object that propagates notification that operations should be canceled.</param>
    /// <returns>
    /// A <see cref="Task"/> that resolves to a <see cref="ValidationResult"/> instance.
    /// </returns>
    public async Task<ValidationResult?> ValidateAsync(object? value, ValidationContext validationContext, CancellationToken cancellationToken)
    {
        // Execute the custom asynchronous predicate.
        var isValid = await predicate(value, cancellationToken);

        if (isValid)
        {
            return ValidationResult.Success!;
        }

        // If validation fails, return a new ValidationResult with an error message.
        return new ValidationResult(
            ErrorMessage ?? "The asynchronous validation predicate did not satisfy the condition."
        );
    }
}