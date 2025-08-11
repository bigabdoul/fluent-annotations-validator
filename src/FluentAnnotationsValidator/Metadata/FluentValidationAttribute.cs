using FluentAnnotationsValidator.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Base class for all FluentValidation attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public abstract class FluentValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationAttribute"/> class with no error message.
    /// </summary>
    public FluentValidationAttribute() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationAttribute"/> class with a specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message to use when validation fails.</param>
    protected FluentValidationAttribute(string errorMessage) : base(errorMessage) { }

    /// <summary>
    /// Optional message resolver to use for resolving validation messages.
    /// </summary>
    public virtual IValidationMessageResolver? MessageResolver { get; set; }
}
