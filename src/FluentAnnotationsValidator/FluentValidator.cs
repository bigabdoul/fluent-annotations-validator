using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

using Core.Interfaces;
using Core.Results;
using Runtime;
using Runtime.Interfaces;

/// <summary>
/// A fluent validator that performs validation on an object of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// This class serves as the core entry point for the validation process. It uses the
/// configured rules from <see cref="ValidationRuleGroupRegistry"/> and resolves error
/// messages using the <see cref="IValidationMessageResolver"/>.
/// </remarks>
/// <param name="registry">The configured validation behavior options for the application.</param>
/// <param name="resolver">The service responsible for resolving validation error messages.</param>
/// <typeparam name="T">The type of the object instance to be validated.</typeparam>
public class FluentValidator<T>(IRuleRegistry registry, IValidationMessageResolver resolver) : IFluentValidator<T>
{
    private IRuleRegistry _ruleRegistry = registry;
    private IValidationMessageResolver _messageResolver = resolver;
    private ValidationContext? _validationContext;

    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no rules were configured for the type <typeparamref name="T"/>.
    /// </exception>
    public virtual FluentValidationResult Validate(T instance) => Validate(instance, throwWhenNoRules: true);

    /// <summary>
    /// Validates the specified instance, optionally throwing an exception
    /// when no rules were configured for the type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="throwWhenNoRules">
    /// <see langword="true"/> to throw an exception if no rules were configured
    /// for <typeparamref name="T"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>A <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    public virtual FluentValidationResult Validate(T instance, bool throwWhenNoRules)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var failures = new List<FluentValidationFailure>();
        var items = _validationContext?.Items;

        if (throwWhenNoRules)
        {
            var ruleCount = 0;

            foreach (var rulesForMember in _ruleRegistry.GetRulesByMember(typeof(T)))
            {
                var rules = rulesForMember.ToList();
                ruleCount += rules.Count;
                ValidateAll(rules, rulesForMember.Key);
            }

            if (ruleCount == 0)
                throw new InvalidOperationException(
                        $"No rules found for the type {typeof(T).Name}. Are you sure you invoked " +
                        $"{nameof(IFluentTypeValidator<T>)}<{typeof(T).Name}>.{nameof(IFluentTypeValidator<T>.Build)}() " +
                        "before calling this method?");
        }
        else
        {
            foreach (var rulesForMember in _ruleRegistry.GetRulesByMember(typeof(T)))
            {
                ValidateAll(rulesForMember, rulesForMember.Key);
            }
        }

        return new(failures);

        void ValidateAll(IEnumerable<IValidationRule> rules, MemberInfo member)
        {
            var validationErrorResults = rules.Validate(instance, member, _messageResolver, _ruleRegistry, items);
            foreach (var error in validationErrorResults)
            {
                failures.Add(error.Failure ?? new(error));
            }
        }
    }

    /// <inheritdoc />
    public virtual FluentValidationResult Validate(ValidationContext context)
    {
        _validationContext = context;
        return Validate((T)context.ObjectInstance);
    }

    /// <summary>
    /// Asynchronously validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to a <see cref="FluentValidationResult"/> object containing any validation failures.</returns>
    public virtual async Task<FluentValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var failures = new List<FluentValidationFailure>();
        var items = _validationContext?.Items;

        foreach (var rulesForMember in _ruleRegistry.GetRulesByMember(typeof(T)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validationErrorResults = await rulesForMember.ValidateAsync(instance,
                rulesForMember.Key, _messageResolver, _ruleRegistry, items, cancellationToken: cancellationToken);

            foreach (var error in validationErrorResults)
            {
                failures.Add(error.Failure ?? new(error));
            }
        }

        return new(failures);
    }

    /// <inheritdoc />
    public virtual Task<FluentValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellationToken = default)
    {
        _validationContext = context;
        return ValidateAsync((T)context.ObjectInstance, cancellationToken);
    }

    /// <summary>
    /// Determines whether the validator can validate instances of the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the validator can validate the type; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool CanValidateInstancesOfType(Type type)
        => type is not null && (typeof(T) == type || type.IsSubclassOf(typeof(T)));

    /// <inheritdoc/>
    public void SetRuleRegistry(IRuleRegistry value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _ruleRegistry = value;
    }

    /// <inheritdoc/>
    public void SetMessageResolver(IValidationMessageResolver value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _messageResolver = value;
    }
}
