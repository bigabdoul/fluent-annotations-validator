using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Runtime;

using Annotations;
using Core;
using Core.Extensions;
using Core.Interfaces;
using Runtime.Interfaces;

/// <summary>
/// Implements a fluent, type-safe contract for building validation rules for a specific
/// property or field of a model.
/// </summary>
/// <typeparam name="T">The type of the object instance being validated.</typeparam>
/// <typeparam name="TProp">The type of the property being validated.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationRuleBuilder{T, TProp}"/> class.
/// </remarks>
/// <param name="currentRule">The <see cref="PendingRule{T}"/> for which the member is being configured.</param>
/// <param name="registry">The validation rule group registry to use.</param>
public class ValidationRuleBuilder<T, TProp>(PendingRule<T> currentRule, IValidationRuleGroupRegistry registry) : IValidationRuleBuilder<T, TProp>
{
    private Predicate<T>? _whenCondition;
    private Func<T, CancellationToken, Task<bool>>? _whenAsyncCondition;
    private bool? _isAsync = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationRuleBuilder{T, TProp}"/>
    /// class using the specified parameters.
    /// </summary>
    /// <param name="currentRule">The <see cref="PendingRule{T}"/> for which the member is being configured.</param>
    /// <param name="registry">The validation rule registry to use.</param>
    /// <param name="isAsync">Determines whether the current instance uses asynchronous methods and validators.</param>
    protected ValidationRuleBuilder(PendingRule<T> currentRule, IValidationRuleGroupRegistry registry, bool isAsync)
        : this(currentRule, registry)
    {
        _isAsync = isAsync;
    }

    /// <summary>
    /// Gets the internally configured rules list.
    /// </summary>
    protected virtual internal List<IValidationRule<T>> Rules { get; } = [];

    /// <summary>
    /// Gets the internally configured child rules list.
    /// </summary>
    protected virtual internal List<IValidationRule<TProp>> ChildRules { get; } = [];

    /// <summary>
    /// Gets or sets the <see cref="PendingRule{T}"/> for which the member is being configured.
    /// </summary>
    protected virtual PendingRule<T> CurrentRule => currentRule;

    /// <inheritdoc cref="IValidationRuleBuilder.Member"/>
    public virtual Expression Member => currentRule.Expression;

    /// <inheritdoc/>
    IValidationRule IValidationRuleBuilder.CurrentRule => CurrentRule;

    /// <inheritdoc/>
    public bool IsAsync => _isAsync ?? false;

    /// <inheritdoc/>
    public IValidationRuleGroupRegistry Registry => registry;

    /// <inheritdoc cref="IValidationRuleBuilder.GetRules"/>
    public virtual IReadOnlyCollection<IValidationRule> GetRules() =>
        Rules.Union(ChildRules.Cast<IValidationRule>()).ToList().AsReadOnly();

    /// <inheritdoc cref="IValidationRuleBuilder.RemoveRules(Predicate{IValidationRule})"/>
    public virtual int RemoveRules(Predicate<IValidationRule> condition) => Rules.RemoveAll(condition);

    /// <summary>
    /// Adds a conditional group of rules that will only be executed if the specified predicate is true.
    /// </summary>
    /// <param name="condition">A predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no rules are configured within the <paramref name="configure"/> action.</exception>
    public virtual IValidationRuleBuilder<T, TProp> When(Predicate<T> condition, Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        SetSynchronous();

        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule, registry, isAsync: false);

        configure(nestedBuilder);
        
        if (nestedBuilder.Rules.Count == 0)
        {
            // Add default role that evaluates the specified condition.
            Rules.Add(new ValidationRule<T>(condition, currentRule.Expression));
        }
        else
        {
            foreach (var nestedRule in nestedBuilder.Rules)
            {
                var originalPredicate = nestedRule.Condition;

                // Compose a new predicate that checks the 'When' condition first,
                // and then the original nested rule's predicate.
                nestedRule.Condition = instance => condition(instance) && originalPredicate(instance);

                nestedRule.SetShouldValidate(condition);

                Rules.Add(nestedRule);
            }
        }

        _whenCondition = condition;

        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that will only be executed if the specified asynchronous predicate is true.
    /// </summary>
    /// <param name="condition">An asynchronous predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no rules are configured within the <paramref name="configure"/> action.</exception>
    public virtual IValidationRuleBuilder<T, TProp> WhenAsync(Func<T, CancellationToken, Task<bool>> condition, Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        SetAsynchronous();

        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule, registry, isAsync: true);

        configure(nestedBuilder);

        if (nestedBuilder.Rules.Count == 0)
        {
            Rules.Add(new ValidationRule<T>(_ => true, currentRule.Expression) { AsyncCondition = condition });
        }
        else
        {
            Task<bool> ShouldApplyAsync(T instance, CancellationToken cancellationToken)
                    => condition(instance, cancellationToken);

            foreach (var nestedRule in nestedBuilder.Rules)
            {
                var originalAsyncPredicate = nestedRule.AsyncCondition;

                // Compose a new asynchronous predicate that chains the outer and inner conditions.
                nestedRule.AsyncCondition = async (instance, cancellationToken) =>
                    await ShouldApplyAsync(instance, cancellationToken) &&
                    (originalAsyncPredicate is null || await originalAsyncPredicate(instance, cancellationToken));

                nestedRule.SetShouldAsyncValidate(ShouldApplyAsync);

                Rules.Add(nestedRule);
            }
        }


        _whenAsyncCondition = condition;
        _isAsync = true;

        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that will only be executed if the specified predicate is true.
    /// This is a semantic alias for <see cref="When(Predicate{T}, Action{IValidationRuleBuilder{T, TProp}})"/>.
    /// </summary>
    /// <param name="condition">A predicate that determines whether to apply the nested rules.</param>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    public virtual IValidationRuleBuilder<T, TProp> Must(Predicate<T> condition, Action<IValidationRuleBuilder<T, TProp>> configure)
        => When(condition, configure);

    /// <summary>
    /// Adds a simple validation rule using a synchronous predicate that evaluates a property's value.
    /// </summary>
    /// <param name="condition">A function that performs the validation on the property's value.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    public virtual IValidationRuleBuilder<T, TProp> Must(Predicate<TProp> condition)
    {
        SetSynchronous();

        var member = currentRule.Expression;

        bool composedCondition(T model)
        {
            var value = member.GetMemberValue(model!);
            return condition((TProp)value!);
        }

        var rule = currentRule.CreateRuleFromPending(member.GetMemberInfo(),
            attribute: new MustAttribute<TProp>(condition),
            composedCondition);

        rule.SetShouldValidate(_ => true);

        Rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Adds a simple validation rule using an asynchronous predicate that evaluates a property's value.
    /// </summary>
    /// <param name="condition">An asynchronous function that performs the validation on the property's value.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    public virtual IValidationRuleBuilder<T, TProp> MustAsync(Func<TProp?, CancellationToken, Task<bool>> condition)
    {
        SetAsynchronous();

        var member = currentRule.Expression;

        async Task<bool> composedAsyncCondition(T model, CancellationToken cancellationToken)
        {
            var value = member.GetMemberValue(model!);
            return await condition((TProp)value!, cancellationToken);
        }

        var rule = currentRule.CreateRuleFromPending(member.GetMemberInfo(),
            attribute: new AsyncValidationAttribute((prop, cancellation) => condition((TProp?)prop, cancellation)),
            asyncCondition: composedAsyncCondition);

        rule.SetShouldAsyncValidate((_, _) => Task.FromResult(true));

        Rules.Add(rule);

        _isAsync = true;
        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that are executed if the previous
    /// <see cref="When(Predicate{T}, Action{IValidationRuleBuilder{T, TProp}})"/>
    /// or <see cref="Must(Predicate{T}, Action{IValidationRuleBuilder{T, TProp}})"/> condition is false.
    /// </summary>
    /// <param name="configure">An action to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is not chained after a <see cref="When"/> or 
    /// <see cref="Must(Predicate{T}, Action{IValidationRuleBuilder{T, TProp}})"/> 
    /// call that accepts a configure action.
    /// </exception>
    public virtual IValidationRuleBuilder<T, TProp> Otherwise(Action<IValidationRuleBuilder<T, TProp>> configure)
    {
        SetSynchronous();

        var lastCondition = _whenCondition
            ?? throw new InvalidOperationException(
                "Otherwise(...) must follow a call to a When(...) or Must(...) method that accepts a configure action."
            );

        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule, registry, false);

        configure(nestedBuilder);
        bool ShouldApply(T instance) => !lastCondition(instance);

        if (nestedBuilder.Rules.Count == 0)
        {
            // Add default role that evaluates the specified condition.
            Rules.Add(new ValidationRule<T>(ShouldApply, currentRule.Expression));
        }
        else
        {
            foreach (var nestedRule in nestedBuilder.Rules)
            {
                // Compose a new predicate that checks the inverse of the 'When' condition,
                // and then the original nested rule's predicate.
                var originalPredicate = nestedRule.Condition;
                nestedRule.Condition = instance => ShouldApply(instance) && originalPredicate(instance);

                nestedRule.SetShouldValidate(ShouldApply);

                Rules.Add(nestedRule);
            }
        }

        foreach (var childRule in nestedBuilder.ChildRules)
        {
            ChildRules.Add(childRule);
        }

        return this;
    }

    /// <summary>
    /// Adds a conditional group of rules that are executed if the previous
    /// <see cref="WhenAsync(Func{T, CancellationToken, Task{bool}}, Action{IValidationRuleBuilder{T, TProp}})"/>
    /// condition is false.
    /// </summary>
    /// <param name="configure">A function to configure the nested rules within this conditional scope.</param>
    /// <returns>The current rule builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is not chained after a <see cref="WhenAsync"/> call.
    /// </exception>
    public virtual IValidationRuleBuilder<T, TProp> OtherwiseAsync(Func<IValidationRuleBuilder<T, TProp>, Task> configure)
    {
        SetAsynchronous();

        var lastAsyncCondition = _whenAsyncCondition
            ?? throw new InvalidOperationException("OtherwiseAsync(...) must follow a call to a WhenAsync(...) method that accepts a configure action.");

        var nestedBuilder = new ValidationRuleBuilder<T, TProp>(currentRule, registry, isAsync: true);

        configure(nestedBuilder);

        async Task<bool> ShouldApplyAsync(T instance, CancellationToken cancellationToken)
        {
            return !await lastAsyncCondition(instance, cancellationToken);
        }

        if (nestedBuilder.Rules.Count == 0)
        {
            // Add default role that evaluates the specified condition.
            Rules.Add(new ValidationRule<T>(_ => true, currentRule.Expression) { AsyncCondition = ShouldApplyAsync });
        }
        else
        {
            foreach (var nestedRule in nestedBuilder.Rules)
            {
                // Compose a new asynchronous predicate that chains the outer and inner conditions.
                var originalAsyncCondition = nestedRule.AsyncCondition;

                nestedRule.AsyncCondition = async (instance, cancellationToken) =>
                    await ShouldApplyAsync(instance, cancellationToken) &&
                    (originalAsyncCondition is null || await originalAsyncCondition(instance, cancellationToken));

                nestedRule.SetShouldAsyncValidate(ShouldApplyAsync);
                Rules.Add(nestedRule);
            }
        }

        foreach (var childRule in nestedBuilder.ChildRules)
        {
            ChildRules.Add(childRule);
        }

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.SetValidator(ValidationAttribute)"/>
    public virtual IValidationRuleBuilder<T, TProp> SetValidator(ValidationAttribute attribute)
    {
        var member = currentRule.Expression.GetMemberInfo();
        var rule = currentRule.CreateRuleFromPending(member,
            attribute,
            currentRule.Condition,
            currentRule.AsyncCondition);

        Rules.Add(rule);

        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.WithMessage(string?)"/>
    public virtual IValidationRuleBuilder<T, TProp> WithMessage(string? message)
    {
        Rules.Last().Message = message;
        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.WithMessage(Func{T, string})"/>
    public virtual IValidationRuleBuilder<T, TProp> WithMessage(Func<T, string> messageResolver)
    {
        Rules.Last().MessageResolver = instance => messageResolver.Invoke((T)instance);
        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.OverridePropertyName(string)"/>
    public virtual IValidationRuleBuilder<T, TProp> OverridePropertyName(string propertyName)
    {
        Rules.Last().PropertyName = propertyName;
        return this;
    }

    /// <inheritdoc cref="IValidationRuleBuilder{T, TProp}.BeforeValidation(PreValidationValueProviderDelegate{T, TProp})"/>
    public virtual IValidationRuleBuilder<T, TProp> BeforeValidation(PreValidationValueProviderDelegate<T, TProp> configure)
    {
        Rules.Last().ConfigureBeforeValidation = (instance, member, memberValue) =>
            configure.Invoke((T)instance, member, (TProp?)memberValue);
        return this;
    }

    /// <inheritdoc/>
    IValidationRuleBuilder IValidationRuleBuilder.SetValidator(ValidationAttribute attribute)
    {
        return SetValidator(attribute);
    }

    /// <inheritdoc/>
    public virtual void AddRule(IValidationRule<T> rule)
    {
        Rules.Add(rule);
    }

    /// <inheritdoc/>
    public virtual void AddChildRule(IValidationRule<TProp> rule) => ChildRules.Add(rule);

    void IValidationRuleBuilder.AddRule(IValidationRule rule)
    {
        if (rule is IValidationRule<T> strongRule)
            AddRule(strongRule);
        else
            throw new InvalidOperationException($"Cannot add the specified rule {rule} to the rules register.");
    }

    private void SetAsynchronous()
    {
        _isAsync ??= true;
    }

    private void SetSynchronous()
    {
        _isAsync ??= false;
    }
}
