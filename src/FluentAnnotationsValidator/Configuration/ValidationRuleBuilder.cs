using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Implements a fluent, type-safe contract for building validation rules for a specific
/// property or field of a model.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TProp">The type of the property being validated.</typeparam>
/// <param name="currentRule">The <see cref="PendingRule{T}"/> for which the member is being configured.</param>
public class ValidationRuleBuilder<T, TProp>(PendingRule<T> currentRule) : IValidationRuleBuilder<T, TProp>
{
    private Func<T, bool>? _whenPredicate;
    private Func<T, CancellationToken, Task<bool>>? _whenAsyncPredicate;

    /// <summary>
    /// Gets the internally configured rules list.
    /// </summary>
    protected internal List<ConditionalValidationRule> Rules { get; } = [];

    /// <inheritdoc cref="IValidationRuleBuilder.Member"/>
    public Expression Member => currentRule.MemberExpression;

    /// <inheritdoc cref="IValidationRuleBuilder.GetRules"/>
    public IReadOnlyCollection<ConditionalValidationRule> GetRules() => Rules.AsReadOnly();

    /// <inheritdoc cref="IValidationRuleBuilder.RemoveRules(Predicate{ConditionalValidationRule})"/>
    public int RemoveRules(Predicate<ConditionalValidationRule> predicate) => Rules.RemoveAll(predicate);

    /// <summary>
    /// Adds a conditional group of rules that will only be executed if the specified predicate is true.
    /// </summary>
    /// <param name="predicate">A predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no rules are configured within the <paramref name="configure"/> action.</exception>
    public IValidationRuleBuilder<T, TProp> When(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule);

        configure(nestedBuilder);

        if (nestedBuilder.Rules.Count == 0)
        {
            throw new InvalidOperationException("No rules configured within the conditional block. At least one rule must be defined within the 'When' scope.");
        }

        bool ShouldApply(object value) => predicate((T)value);

        foreach (var nestedRule in nestedBuilder.Rules)
        {
            var originalPredicate = nestedRule.Predicate;

            // Compose a new predicate that checks the 'When' condition first,
            // and then the original nested rule's predicate.
            nestedRule.Predicate = model => predicate((T)model) && originalPredicate(model);

            nestedRule.SetShouldApply(ShouldApply);

            Rules.Add(nestedRule);
        }

        _whenPredicate = predicate;

        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that will only be executed if the specified asynchronous predicate is true.
    /// </summary>
    /// <param name="predicate">An asynchronous predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no rules are configured within the <paramref name="configure"/> action.</exception>
    public IValidationRuleBuilder<T, TProp> WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule);

        configure(nestedBuilder);

        if (nestedBuilder.Rules.Count == 0)
        {
            throw new InvalidOperationException("No rules configured within the conditional block. At least one rule must be defined within the 'WhenAsync' scope.");
        }

        Task<bool> ShouldApplyAsync(object value, CancellationToken cancellationToken) => predicate((T)value, cancellationToken);

        foreach (var nestedRule in nestedBuilder.Rules)
        {
            var originalPredicate = nestedRule.Predicate;
            var originalAsyncPredicate = nestedRule.AsyncPredicate;

            // Compose a new asynchronous predicate that chains the outer and inner conditions.
            nestedRule.AsyncPredicate = async (model, cancellationToken) =>
            {
                var outerCondition = await predicate((T)model, cancellationToken);
                if (!outerCondition)
                {
                    return false;
                }

                // If the inner rule has its own async predicate, use it.
                if (originalAsyncPredicate is not null)
                {
                    return await originalAsyncPredicate((T)model, cancellationToken);
                }

                // Otherwise, fall back to the synchronous predicate.
                return originalPredicate(model);
            };

            nestedRule.SetShouldApplyAsync(ShouldApplyAsync);

            Rules.Add(nestedRule);
        }

        _whenAsyncPredicate = predicate;

        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that will only be executed if the specified predicate is true.
    /// This is a semantic alias for <see cref="When(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/>.
    /// </summary>
    /// <param name="predicate">A predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    public IValidationRuleBuilder<T, TProp> Must(Func<T, bool> predicate, Action<IValidationRuleBuilder<T, TProp>> configure)
        => When(predicate, configure);

    /// <summary>
    /// Adds a simple validation rule using a synchronous predicate that evaluates a property's value.
    /// </summary>
    /// <param name="predicate">A function that performs the validation on the property's value.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    public IValidationRuleBuilder<T, TProp> Must(Func<TProp, bool> predicate)
    {
        var member = currentRule.MemberExpression;

        bool composedPredicate(T model)
        {
            var value = member.GetMemberValue(model!);
            return predicate((TProp)value!);
        }

        var rule = currentRule.CreateRuleFromPending(member.GetMemberInfo(),
            attribute: new MustAttribute<TProp>(predicate),
            composedPredicate);

        rule.SetShouldApply(_ => true);

        Rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Adds a simple validation rule using an asynchronous predicate that evaluates a property's value.
    /// </summary>
    /// <param name="predicate">An asynchronous function that performs the validation on the property's value.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    public IValidationRuleBuilder<T, TProp> MustAsync(Func<TProp, CancellationToken, Task<bool>> predicate)
    {
        var member = currentRule.MemberExpression;

        async Task<bool> composedAsyncPredicate(T model, CancellationToken cancellationToken)
        {
            var value = member.GetMemberValue(model!);
            return await predicate((TProp)value!, cancellationToken);
        }

        var rule = currentRule.CreateRuleFromPending(member.GetMemberInfo(),
            attribute: new MustAttribute<TProp>(null!),
            asyncPredicate: composedAsyncPredicate);

        rule.SetShouldApplyAsync((_1, _2) => Task.FromResult(true));

        Rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that are executed if the previous
    /// <see cref="When(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/>
    /// or <see cref="Must(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/> condition is false.
    /// </summary>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is not chained after a <see cref="When"/> or 
    /// <see cref="Must(Func{T, bool}, Action{IValidationRuleBuilder{T, TProp}})"/> 
    /// call that accepts a configure action.
    /// </exception>
    public IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        var lastCondition = _whenPredicate
            ?? throw new InvalidOperationException(
                "Otherwise(...) must follow a call to a When(...) or Must(...) method that accepts a configure action."
            );

        var nestedConfigurator = new ValidationRuleBuilder<T, TProp>(currentRule);

        configure(nestedConfigurator);

        bool ShouldApply(object value) => !lastCondition((T)value);

        foreach (var nestedRule in nestedConfigurator.Rules)
        {
            var originalPredicate = nestedRule.Predicate;
            nestedRule.Predicate = model => ShouldApply(model) && originalPredicate(model);
            nestedRule.SetShouldApply(ShouldApply);
            Rules.Add(nestedRule);
        }

        nestedConfigurator.Rules.Clear();
        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.AddRuleFromAttribute(ValidationAttribute)"/>
    public IValidationRuleBuilder<T, TProp> AddRuleFromAttribute(ValidationAttribute attribute)
    {
        var member = currentRule.MemberExpression.GetMemberInfo();
        var rule = currentRule.CreateRuleFromPending(member,
            attribute,
            currentRule.Predicate,
            currentRule.AsyncPredicate);

        Rules.Add(rule);

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.WithMessage(string?)"/>
    public IValidationRuleBuilder<T, TProp> WithMessage(string? message)
    {
        Rules.Last().Message = message;
        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.BeforeValidation(PreValidationValueProviderDelegate{T, TProp})"/>
    public IValidationRuleBuilder<T, TProp> BeforeValidation(PreValidationValueProviderDelegate<T, TProp> configure)
    {
        Rules.Last().ConfigureBeforeValidation = (instance, member, memberValue) =>
            configure.Invoke((T)instance, member, (TProp?)memberValue);
        return this;
    }
}
