using FluentAnnotationsValidator.Results;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines a contract for a validation attribute that performs asynchronous validation.
/// </summary>
/// <remarks>
/// This interface should be used for validation attributes that require asynchronous operations,
/// such as database lookups or API calls. The <see cref="ValidationResultAggregator"/> will
/// recognize this interface and call the asynchronous method, awaiting the result to prevent deadlocks.
/// </remarks>
public interface IAsyncValidationAttribute
{
    /// <summary>
    /// Asynchronously validates the specified value with the given validation context.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information for the validation.</param>
    /// <param name="cancellationToken">An object that propagates notification that operations should be canceled.</param>
    /// <returns>
    /// A <see cref="Task"/> that resolves to a <see cref="ValidationResult"/> instance.
    /// </returns>
    Task<ValidationResult?> ValidateAsync(object? value, ValidationContext validationContext, CancellationToken cancellationToken);
}
