namespace FluentAnnotationsValidator.Core.Interfaces;

/// <summary>
/// Represents a runtime-aware validation attribute that participates in fluent rule composition and execution.
/// </summary>
public interface IFluentValidationAttribute
{
    /// <summary>
    /// Gets or sets the message resolver used to localize or customize validation error messages.
    /// </summary>
    IValidationMessageResolver? MessageResolver { get; set; }

    /// <summary>
    /// Gets or sets the validation rule associated with this attribute, used during runtime evaluation.
    /// </summary>
    IValidationRule? Rule { get; set; }

    /// <summary>
    /// Gets or sets the rule registry that manages rule discovery and grouping for this attribute.
    /// </summary>
    IRuleRegistry? RuleRegistry { get; set; }
}