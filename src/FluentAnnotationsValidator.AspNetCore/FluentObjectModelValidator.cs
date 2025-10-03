using FluentAnnotationsValidator.Core.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FluentAnnotationsValidator.AspNetCore;

/// <summary>
/// An implementation of <see cref="IObjectModelValidator"/> that delegates validation to <see cref="IFluentValidator{T}"/> at runtime.
/// </summary>
/// <remarks>
/// This validator dynamically resolves the appropriate <c><see cref="FluentValidationFilter{T}"/></c> for the incoming model type
/// and populates <see cref="ModelStateDictionary"/> with any validation errors.
/// </remarks>
public class FluentObjectModelValidator(IServiceProvider services) : IObjectModelValidator
{
    #region Extensibility points

    /// <summary>
    /// An optional delegate used to format each <see cref="FluentValidationFailure"/> into a string for model state reporting.
    /// </summary>
    /// <remarks>
    /// If not set, the default formatter uses <see cref="FluentValidationFailure.Describe(string?)"/> with <see cref="ErrorDescriptionSeparator"/>.
    /// </remarks>
    public Func<FluentValidationFailure, string>? DescribeError { get; set; }

    /// <summary>
    /// An optional separator string used when formatting error descriptions via <see cref="FluentValidationFailure.Describe(string?)"/>.
    /// </summary>
    /// <remarks>
    /// This value is ignored if <see cref="DescribeError"/> is explicitly set.
    /// </remarks>
    public string? ErrorDescriptionSeparator { get; set; }

    #endregion
    
    /// <summary>
    /// Validates the specified model using a resolved <see cref="IFluentValidator{T}"/> instance and populates <see cref="ModelStateDictionary"/> with formatted errors.
    /// </summary>
    /// <param name="actionContext">The context for the current action.</param>
    /// <param name="validationState">Optional validation state dictionary (not used).</param>
    /// <param name="prefix">The prefix to apply to model binding keys (not used).</param>
    /// <param name="model">The model object to validate.</param>
    /// <remarks>
    /// This method dynamically resolves the appropriate validator for the model type and applies validation.
    /// If <see cref="DescribeError"/> is set, it is used to format each error; otherwise, <see cref="FluentValidationFailure.Describe(string?)"/> is used.
    /// Validation errors are added to <see cref="ActionContext.ModelState"/> using the property name and formatted message.
    /// </remarks>
    public virtual void Validate(ActionContext actionContext, ValidationStateDictionary? validationState, string prefix, object? model)
    {
        if (model == null) return;

        var validatorType = typeof(IFluentValidator<>).MakeGenericType(model.GetType());
        var validator = (IFluentValidator?)services.GetService(validatorType);

        if (validator is null) return;

        var result = validator.Validate(new(model));
        Func<FluentValidationFailure, string> errorDescriptor = DescribeError ?? (err => err.Describe(ErrorDescriptionSeparator));

        foreach (var error in result.Errors)
        {
            actionContext.ModelState.AddModelError(error.PropertyName, errorDescriptor(error));
        }
    }
}
