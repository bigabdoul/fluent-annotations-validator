using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Annotations;

using Core.Interfaces;
using Core.Results;

/// <summary>
/// Base class for all FluentValidation attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public abstract class FluentValidationAttribute : ValidationAttribute, IValidationResult, IFluentValidationAttribute
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
    /// Gets or sets the rule registry for validator resolution.
    /// </summary>
    public IRuleRegistry? RuleRegistry { get; set; }

    /// <summary>
    /// Gets the list of validation failures.
    /// </summary>
    public List<FluentValidationFailure> Errors { get; } = [];

    /// <summary>
    /// Optional message resolver to use for resolving validation messages.
    /// </summary>
    public virtual IValidationMessageResolver? MessageResolver { get; set; }

    /// <summary>
    /// Gets or sets the validation rule associated with this attribute.
    /// The value of this property is intended to be passed to message resolver.
    /// </summary>
    public virtual IValidationRule? Rule { get; set; }

    /// <summary>
    /// Generates a <see cref="ValidationResult"/> based on a list of validation errors.
    /// </summary>
    /// <param name="errors">A list of <see cref="FluentValidationFailure"/> objects containing validation errors.</param>
    /// <returns>
    /// <para>
    /// <see cref = "ValidationResult.Success" /> if the list of errors is empty.
    /// </para>
    /// <para>
    /// Otherwise, a new <see cref = "ValidationResult" /> with a summary message
    /// and the list of errors added to the class's <see cref="Errors"/> collection.
    /// </para>
    /// </returns>
    protected virtual ValidationResult? GetValidationResult(List<FluentValidationFailure> errors)
    {
        if (errors.Count == 0) return ValidationResult.Success;
        Errors.AddRange(errors);
        return new ValidationResult($"Validation errors occurred: {errors.Count}. See Errors for details.");
    }

    /// <summary>
    /// Gets a failed <see cref="ValidationResult"/> instance with a resolved error message,
    /// falling back to the attribute's default message if a resolver is not provided.
    /// </summary>
    /// <param name="attribute">The <see cref="ValidationAttribute"/> that failed.</param>
    /// <param name="validationContext">The context of the validation operation.</param>
    /// <param name="messageResolver">
    /// An optional message resolver to use for retrieving a localized or custom error message.
    /// </param>
    /// <returns>A new <see cref="ValidationResult"/> representing the validation failure.</returns>
    protected internal static ValidationResult GetFailedValidationResult(ValidationAttribute attribute, ValidationContext validationContext, IValidationMessageResolver? messageResolver = null)
    {
        ArgumentNullException.ThrowIfNull(validationContext);
        var fieldName = validationContext.DisplayName ?? validationContext.MemberName ?? "field";

        var message = messageResolver?.ResolveMessage
        (
            validationContext.ObjectInstance,
            validationContext.MemberName ?? validationContext.DisplayName ?? fieldName,
            attribute,
            (attribute as FluentValidationAttribute)?.Rule
        ) ?? attribute.FormatErrorMessage(fieldName);

        return new ValidationResult(message, [fieldName]);
    }
}
