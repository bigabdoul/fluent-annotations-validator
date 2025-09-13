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

    /// <summary>
    /// Gets or sets the validation rule associated with this attribute.
    /// The value of this property is intended to be passed to message resolver.
    /// </summary>
    public virtual IValidationRule? Rule { get; set; }
}
