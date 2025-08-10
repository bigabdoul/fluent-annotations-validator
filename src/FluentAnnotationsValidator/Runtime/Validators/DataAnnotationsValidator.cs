using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Results;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace FluentAnnotationsValidator.Runtime.Validators;

/// <summary>
/// Validates an object using rules mapped via the rule registry.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
/// <param name="options"></param>
/// <param name="resolver"></param>
public sealed class DataAnnotationsValidator<T>(ValidationBehaviorOptions options, IValidationMessageResolver resolver) : IValidator<T>
{
    /// <inheritdoc cref="IValidator{T}.Validate(T)"/>
    public ValidationResult Validate(T instance)
    {
        var failures = new List<ValidationFailure>();

        foreach (var (member, rules) in options.EnumerateRules<T>())
        {
            var errors = rules.Validate(typeof(T), instance!, member, resolver);

            foreach (var error in errors)
            {
                var failure = new ValidationFailure(error.Member.Name, error.Message, error.AttemptedValue)
                {
                    CustomState = error.Attribute is null ? null : $"Origin: {error.Attribute.GetType().Name}",
                };
                failures.Add(failure);
            }
        }

        return new ValidationResult(failures);
    }

    /// <inheritdoc cref="IValidator{T}.ValidateAsync(T, CancellationToken)"/>
    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) =>
            Task.FromResult(Validate(instance));

    /// <inheritdoc cref="IValidator{T}.Validate(T)"/>
    public ValidationResult Validate(IValidationContext context)
    {
        if (context is ValidationContext<T> typedContext)
            return Validate(typedContext.InstanceToValidate);

        throw new ArgumentException($"Invalid context type: {context.GetType().Name}", nameof(context));
    }

    /// <inheritdoc cref="IValidator.ValidateAsync(IValidationContext, CancellationToken)"/>
    public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default) =>
        Task.FromResult(Validate(context));

    /// <inheritdoc cref="IValidator.CreateDescriptor"/>
    public IValidatorDescriptor CreateDescriptor() => new FluentValidatorDescriptor();

    /// <inheritdoc cref="IValidator.CanValidateInstancesOfType(Type)"/>
    public bool CanValidateInstancesOfType(Type type) => typeof(T).IsAssignableFrom(type);

    #region FluentValidatorDescriptor

    private sealed class FluentValidatorDescriptor : IValidatorDescriptor
    {
        public IEnumerable<IValidationRule> Rules => [];

        public string GetName(string property) => property;

        public ILookup<string, (IPropertyValidator Validator, IRuleComponent Options)> GetMembersWithValidators() =>
            Enumerable.Empty<(IPropertyValidator, IRuleComponent)>().ToLookup(_ => string.Empty);

        public IEnumerable<(IPropertyValidator Validator, IRuleComponent Options)> GetValidatorsForMember(string name) => [];

        public IEnumerable<IValidationRule> GetRulesForMember(string name) => [];
    }

    #endregion
}
