using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Provides a fluent, type-safe configuration surface for defining validation logic
/// on a specific model type using <see cref="ValidationRuleGroupRegistry"/>.
/// </summary>
/// <typeparam name="T">The model or DTO type being configured.</typeparam>
/// <param name="root">
/// The root-level validation configurator allowing to transition 
/// to another <see cref="FluentTypeValidator{T}"/>.
/// </param>
/// <remarks>
/// This configurator allows chaining multiple rules and metadata overrides such as custom messages,
/// resource keys, and validation keys. All configured rules are buffered and registered during the final
/// <c>Build()</c> call to ensure expressive, discoverable configuration flows.
///
/// Typical usages:
/// <code>
/// services.AddFluentAnnotations(
///     localizerFactory: factory => new(typeof(FluentValidationMessages), CultureInfo.GetCultureInfo("fr")),
///     configure: config =>
///     {
///         var registrationValidator = config.For&lt;RegisterModel>();
///
///         registrationValidator.RuleFor(x => x.BirthDate)
///             .When(x => x.BirthDate.HasValue, user => user.Must(BeAtLeast13));
///
///         registrationValidator.RuleFor(x => x.Email)
///             .Required()
///             .EmailAddress();
///
///         registrationValidator.RuleFor(x => x.Password)
///             .Must(BeComplexPassword);
///             
///         registrationValidator.RuleFor(x => x.PhoneNumber)
///             .When(x => !string.IsNullOrEmpty(x.PhoneNumber), number => number.MinimumLength(9).Must(BeValidPhoneNumber));
///
///         registrationValidator.Build();
///
///     },
///     targetAssembliesTypes: typeof(RegisterModel));
/// </code>
/// </remarks>
public class FluentTypeValidator<T>(FluentTypeValidatorRoot root)
    : FluentTypeValidatorBase(typeof(T)), IFluentTypeValidator<T>
{
    private static readonly Predicate<T> AlwaysTruePredicate = _ => true;

    private readonly HashSet<PendingRule<T>> _pendingRules = [];
    private readonly List<IValidationRuleBuilder> _validationRuleBuilders = [];

    private PendingRule<T>? _currentRule;
    private bool _useConventionalKeys = true;
    private string? _fallbackMessage;
    private bool? _disableConfigurationEnforcement;

    /// <summary>
    /// This field is used to store rules from the last <see cref="Build"/> call.
    /// It is crucial for the method
    /// <see cref="ValidationRuleBuilderExtensions.ChildRules{T, TProp}(IValidationRuleBuilder{T, TProp}, Action{FluentTypeValidator{TProp}})"/>
    /// since one might call <see cref="Build"/> too early after configuring the instance,
    /// therefore discarding the rules that were built and meant to be retrieved by this method.
    /// </summary>
    protected readonly List<IValidationRule> RulesFromLastBuild = [];

    /// <summary>
    /// Gets the validation rule group registry.
    /// </summary>
    public IValidationRuleGroupRegistry Registry => root.Registry;

    /// <inheritdoc cref="IFluentTypeValidator{T}.WithValidationResource{TResource}()"/>
    public virtual FluentTypeValidator<T> WithValidationResource<TResource>()
    {
        AssignCultureTo(typeof(TResource));
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.WithValidationResource(Type?)"/>
    public virtual FluentTypeValidator<T> WithValidationResource(Type? resourceType)
    {
        AssignCultureTo(resourceType);
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.WithCulture(CultureInfo)"/>
    public virtual FluentTypeValidator<T> WithCulture(CultureInfo culture)
    {
        Culture = culture;
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.For{TNext}"/>
    public FluentTypeValidator<TNext> For<TNext>()
    {
        CommitCurrentRule();
        GlobalRegistry.Default.Register(typeof(T), this);
        return root.For<TNext>();
    }

    #region Rules Management

    /// <summary>
    /// Determines whether the current configurator contains 
    /// neither pending rules, nor validation rule builders.
    /// </summary>
    protected virtual bool IsEmpty => _pendingRules.Count == 0 && _validationRuleBuilders.Count == 0;

    /// <inheritdoc cref="IFluentTypeValidator{T}.Rule{TMember}(Expression{Func{T, TMember}})"/>
    public virtual FluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member)
        => Rule(member, RuleDefinitionBehavior.Replace);

    /// <inheritdoc cref="IFluentTypeValidator{T}.Rule{TMember}(Expression{Func{T, TMember}}, RuleDefinitionBehavior)"/>
    public virtual FluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member, RuleDefinitionBehavior behavior)
    {
        CommitCurrentRule();

        if (behavior == RuleDefinitionBehavior.Replace)
            RemoveRulesFor(member);

        _currentRule = new PendingRule<T>(
            member,
            predicate: AlwaysTruePredicate, // Always validate unless overridden by .When(...)
            resourceType: ValidationResourceType,
            culture: Culture,
            fallbackMessage: _fallbackMessage,
            useConventionalKeys: _useConventionalKeys
        );

        MarkAsUnbuilt();
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.Rule{TMember}(Expression{Func{T, TMember}}, Predicate{TMember})"/>
    public virtual FluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must)
        => Rule(member, must, RuleDefinitionBehavior.Replace);

    /// <inheritdoc cref="IFluentTypeValidator{T}.Rule{TMember}(Expression{Func{T, TMember}}, Predicate{TMember}, RuleDefinitionBehavior)"/>
    public virtual FluentTypeValidator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must, RuleDefinitionBehavior behavior)
        => Rule(member, behavior).AddValidator(new MustAttribute<TMember>(must), AlwaysTruePredicate);

    /// <inheritdoc cref="IFluentTypeValidator{T}.RuleFor{TMember}(Expression{Func{T, TMember}})"/>
    public IValidationRuleBuilder<T, TMember> RuleFor<TMember>(Expression<Func<T, TMember>> member)
    {
        CommitCurrentRule();

        var newPendingRule = new PendingRule<T>(
            member,
            predicate: AlwaysTruePredicate, // Always validate unless overridden by .When(...)
            resourceType: ValidationResourceType,
            culture: Culture,
            fallbackMessage: _fallbackMessage,
            useConventionalKeys: _useConventionalKeys
        );

        var configurator = new ValidationRuleBuilder<T, TMember>(newPendingRule, Registry);
        _validationRuleBuilders.Add(configurator);
        MarkAsUnbuilt();
        return configurator;
    }

    /// <summary>
    /// Defines a validation rule for each item in a collection.
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the collection.</typeparam>
    /// <param name="expression">A lambda expression that specifies the collection to validate.</param>
    /// <returns>An instance of a rule builder on which validators can be defined for the elements.</returns>
    public IValidationRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        CommitCurrentRule();

        var newPendingRule = new PendingRule<T>(expression);
        var configurator = new ValidationRuleBuilder<T, TElement>(newPendingRule, Registry);
        _validationRuleBuilders.Add(configurator);
        MarkAsUnbuilt();
        return configurator;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.RemoveRulesFor{TMember}(Expression{Func{T, TMember}})"/>
    public virtual FluentTypeValidator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> member)
    {
        var memberInfo = member.GetMemberInfo();
        _ = Registry.RemoveAll(typeof(T), memberInfo);
        return RemovePendingRules(memberInfo);
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.RemoveRulesFor{TMember, TAttribute}(Expression{Func{T, TMember}})"/>
    public virtual FluentTypeValidator<T> RemoveRulesFor<TMember, TAttribute>(Expression<Func<T, TMember>> member)
        where TAttribute : ValidationAttribute
    {
        var memberInfo = member.GetMemberInfo();

        Registry.RemoveAll<TAttribute>((member, attribute) => memberInfo.AreSameMembers(member));
        RemovePendingRules(memberInfo, typeof(TAttribute));

        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.RemoveRulesFor{TMember}(Expression{Func{T, TMember}}, Type)"/>
    public virtual FluentTypeValidator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> member, Type attributeType)
    {
        var memberInfo = member.GetMemberInfo();
        _ = Registry.RemoveAll(typeof(T), memberInfo, attributeType);
        return RemovePendingRules(memberInfo, attributeType);
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.RemoveRulesExceptFor{TMember}(Expression{Func{T, TMember}})"/>
    public virtual FluentTypeValidator<T> RemoveRulesExceptFor<TMember>(Expression<Func<T, TMember>> member)
    {
        var memberInfo = member.GetMemberInfo();

        _ = Registry.RemoveAll(typeof(T), mi => !memberInfo.AreSameMembers(mi));
        _ = _pendingRules.RemoveWhere(rule => !memberInfo.AreSameMembers(rule.Expression.GetMemberInfo()));
        _ = _validationRuleBuilders.RemoveAll(builder => !memberInfo.AreSameMembers(builder.Member.GetMemberInfo()));

        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.RemovePendingRules(MemberInfo)"/>
    public virtual FluentTypeValidator<T> RemovePendingRules(MemberInfo memberInfo)
    {
        _ = _pendingRules.RemoveWhere(rule => memberInfo.AreSameMembers(rule.Expression.GetMemberInfo()));
        _ = _validationRuleBuilders.RemoveAll(builder => memberInfo.AreSameMembers(builder.Member.GetMemberInfo()));
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.RemovePendingRules(MemberInfo, Type)"/>
    public virtual FluentTypeValidator<T> RemovePendingRules(MemberInfo memberInfo, Type validationAttributeType)
    {
        ArgumentNullException.ThrowIfNull(validationAttributeType);

        // A single counter to track the total number of removed attributes
        var attributesRemoved = 0;

        // Remove attributes from the pending rules collection
        foreach (var rule in _pendingRules)
        {
            // Only process rules for the specified member
            if (memberInfo.AreSameMembers(rule.Expression.GetMemberInfo()))
            {
                // Remove all attributes that match the specified type
                var count = rule.Attributes.RemoveAll(attr => attr.GetType() == validationAttributeType);
                attributesRemoved += count;

#if DEBUG
                Debug.WriteLine($"Removed {count} attribute(s) from pending rule for member '{memberInfo.Name}'.");
#endif
            }
        }

        // Remove attributes from the validation rule builders collection
        foreach (var builder in _validationRuleBuilders)
        {
            // Only process builders for the specified member
            if (memberInfo.AreSameMembers(builder.Member.GetMemberInfo()))
            {
                // Remove rules based on the presence and type of the attribute
                var count = builder.RemoveRules(rule =>
                    rule.HasValidator && rule.Validator!.GetType() == validationAttributeType);

                attributesRemoved += count;

#if DEBUG
                Debug.WriteLine($"Removed {count} attribute(s) from validation rule builder for member '{memberInfo.Name}'.");
#endif
            }
        }

#if DEBUG
        Debug.WriteLine($"Removed a total of {attributesRemoved} attribute(s) from all pending rules.");
#endif

        return this;
    }

    /// <summary>
    /// Clears all pending and registered validation rules for the current model type.
    /// </summary>
    /// <remarks>
    /// This is a destructive operation that resets the configurator's state. It removes:
    /// <list type="bullet">
    ///     <item>All pending rules that have not yet been built.</item>
    ///     <item>All validation rule builders associated with this configurator.</item>
    ///     <item>All rules for this model type from the global options registry.</item>
    /// </list>
    /// This method is useful for scenarios where you need to completely redefine
    /// the validation behavior for a type from scratch.
    /// </remarks>
    /// <returns>The current <see cref="FluentTypeValidator{T}"/> instance for fluent chaining.</returns>
    public virtual FluentTypeValidator<T> ClearRules()
    {
        _pendingRules.Clear();
        _validationRuleBuilders.Clear();
        Registry.RemoveAllForType(typeof(T));
        return this;
    }

    #endregion

    #region When

    /// <summary>
    /// Applies a conditional predicate to the current rule. All subsequent validation rules
    /// in the chain will only be executed if the specified condition is met.
    /// </summary>
    /// <remarks>
    /// This method acts as a logical scope for a group of rules. The condition is evaluated
    /// against the entire model instance, and if it returns <c>false</c>, all
    /// subsequent validation rules chained after this method will be skipped.
    /// </remarks>
    /// <param name="condition">A predicate function that determines whether the following rules should be executed.</param>
    /// <returns>The current configurator for further chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is not chained after a rule-creation method like <see cref="RuleFor{TMember}(Expression{Func{T, TMember}})"/>
    /// or <see cref="Rule{TMember}(Expression{Func{T, TMember}})"/>.
    /// </exception>
    public virtual FluentTypeValidator<T> When(Predicate<T> condition)
    {
        // Applies to the predicate of a rule created with .Rule(...);
        // throws if _currentRule is not defined

        if (_currentRule is null)
            throw new InvalidOperationException("You must create a rule with the .Rule(...) method.");

        // .Rule(...) are attribute-based; therefore, the predicate
        // should only be applicable to the attribute being configured

        if (_currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(_currentRule.Expression);

        _currentRule.Condition = model => condition(model);

        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.When{TMember}(Expression{Func{T, TMember}}, Predicate{T})"/>
    public virtual FluentTypeValidator<T> When<TMember>(Expression<Func<T, TMember>> member, Predicate<T> condition)
    {
        if (_currentRule is null || _currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(member);

        MemberInfo memberInfo;

        if (_currentRule is null || !_currentRule.Expression.GetMemberInfo().AreSameMembers(memberInfo = member.GetMemberInfo()))
        {
            CommitCurrentRule();
            _currentRule = new PendingRule<T>(
                member,
                predicate: model => condition(model),
                resourceType: ValidationResourceType,
                culture: Culture,
                fallbackMessage: _fallbackMessage,
                useConventionalKeys: _useConventionalKeys
            );
            return this;
        }

        // continue configuration of the current rule
        _currentRule.Condition = model => condition(model); // override the predicate, or compose?

        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.WhenAsync(Func{T, CancellationToken, Task{bool}})"/>
    public virtual FluentTypeValidator<T> WhenAsync(Func<T, CancellationToken, Task<bool>> condition)
    {
        if (_currentRule is null)
            throw new InvalidOperationException("You must create a rule with the .Rule(...) method.");

        // .Rule(...) are attribute-based; therefore, the predicate
        // should only be applicable to the attribute being configured

        if (_currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(_currentRule.Expression);

        _currentRule.AsyncCondition = condition;

        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.WhenAsync{TProp}(Expression{Func{T, TProp}}, Func{T, CancellationToken, Task{bool}})"/>
    public virtual FluentTypeValidator<T> WhenAsync<TProp>(Expression<Func<T, TProp>> member, Func<T, CancellationToken, Task<bool>> condition)
    {
        if (_currentRule is null || _currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(member);

        MemberInfo memberInfo;

        if (_currentRule is null || !_currentRule.Expression.GetMemberInfo().AreSameMembers(memberInfo = member.GetMemberInfo()))
        {
            CommitCurrentRule();
            _currentRule = new PendingRule<T>(
                member,
                predicate: AlwaysTruePredicate,
                resourceType: ValidationResourceType,
                culture: Culture,
                fallbackMessage: _fallbackMessage,
                useConventionalKeys: _useConventionalKeys
            );
            return this;
        }

        _currentRule.AsyncCondition = condition;

        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.And{TMember}(Expression{Func{T, TMember}}, Predicate{T})"/>
    public virtual FluentTypeValidator<T> And<TMember>(Expression<Func<T, TMember>> property, Predicate<T> condition)
        => When(property, condition);

    /// <inheritdoc cref="IFluentTypeValidator{T}.Except{TMember}(Expression{Func{T, TMember}})"/>
    public virtual FluentTypeValidator<T> Except<TMember>(Expression<Func<T, TMember>> property)
    {
        CommitCurrentRule();
        return RemoveRulesFor(property);
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.AlwaysValidate{TMember}(Expression{Func{T, TMember}})"/>
    public virtual FluentTypeValidator<T> AlwaysValidate<TMember>(Expression<Func<T, TMember>> property)
        => When(property, AlwaysTruePredicate);

    #endregion

    /// <inheritdoc cref="IFluentTypeValidator{T}.WithMessage(string)"/>
    public virtual FluentTypeValidator<T> WithMessage(string message)
    {
        if (_currentRule is not null)
        {
            if (_currentRule.Attributes.Count > 0)
                _currentRule.Attributes.Last().ErrorMessage = message;
            else
                _currentRule.Message = message;
        }
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.WithKey(string)"/>
    public virtual FluentTypeValidator<T> WithKey(string key)
    {
        if (_currentRule is not null)
            _currentRule.Key = key;
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.Localized(string)"/>
    public virtual FluentTypeValidator<T> Localized(string resourceKey)
    {
        if (_currentRule is not null)
            _currentRule.ResourceKey = resourceKey;
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.DisableConventionalKeys"/>
    public virtual FluentTypeValidator<T> DisableConventionalKeys()
    {
        _useConventionalKeys = false;
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.DisableConfigurationEnforcement(bool)"/>
    public virtual FluentTypeValidator<T> DisableConfigurationEnforcement(bool disableConfigurationEnforcement)
    {
        _disableConfigurationEnforcement = disableConfigurationEnforcement;
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.UseFallbackMessage(string)"/>
    public virtual FluentTypeValidator<T> UseFallbackMessage(string fallbackMessage)
    {
        _fallbackMessage = fallbackMessage;
        if (_currentRule is not null)
            _currentRule.FallbackMessage = _fallbackMessage;
        return this;
    }

    /// <inheritdoc cref="IFluentTypeValidator{T}.BeforeValidation(PreValidationValueProviderDelegate{T})"/>
    public virtual FluentTypeValidator<T> BeforeValidation(PreValidationValueProviderDelegate<T> configure)
    {
        ArgumentNullException.ThrowIfNull(_currentRule);

        object? onPreValidation(object instance, MemberInfo member, object? memberValue) =>
            configure.Invoke((T)instance, member, memberValue);

        EnsureSinglePreValidationValueProvider(_currentRule.Expression.GetMemberInfo(), onPreValidation);

        _currentRule.ConfigureBeforeValidation = onPreValidation;

        return this;
    }

    /// <summary>
    /// Finalizes the configuration of validation rules for the type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>A list of <see cref="IValidationRule"/> objects that were built.</returns>
    /// <remarks>
    /// This method resolves all pending rules and rule builders, registers them with the
    /// central validation behavior options, and performs final checks for consistency.
    /// </remarks>
    public virtual IReadOnlyList<IValidationRule> Build()
    {
        CommitCurrentRule();

        Registry.MarkBuilt(typeof(T), true);

        if (IsEmpty)
        {
            return RulesFromLastBuild.AsReadOnly();
        }

        // A single, unified list to collect all rules before registration
        var registrationList = new List<IValidationRule>();

        // Step 1: Process and transform rules from pending rule collection
        foreach (var pendingRule in _pendingRules)
        {
            var member = pendingRule.Expression.GetMemberInfo();

            if (pendingRule.Attributes.Count > 0)
            {
                foreach (var attr in pendingRule.Attributes)
                {
                    AddRuleToList(pendingRule, member, attr);
                }
            }
            else
            {
                AddRuleToList(pendingRule, member, null);
            }
        }

        // Step 2: Add rules from the rule builders
        AddRuleBuilders(registrationList);

        // Step 3: Register all rules, performing consistency checks
        RegisterRules(registrationList);

        // Clear the temporary collections
        _pendingRules.Clear();
        _validationRuleBuilders.Clear();
        RulesFromLastBuild.Clear();
        RulesFromLastBuild.AddRange(registrationList);

        return registrationList;

        void AddRuleToList(PendingRule<T> pendingRule, MemberInfo member, ValidationAttribute? attr)
        {
            var newRule = pendingRule.CreateRuleFromPending(member, attr);
            newRule.ConfigureBeforeValidation = pendingRule.ConfigureBeforeValidation;
            registrationList.Add(newRule);
        }
    }

    /// <inheritdoc/>
    public void DiscardRulesFromLastBuild() => RulesFromLastBuild?.Clear();

    #region IFluentTypeValidator<T>

    IFluentTypeValidator<T> IFluentTypeValidator<T>.When(Predicate<T> condition)
        => When(condition);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.When<TMember>(Expression<Func<T, TMember>> property, Predicate<T> condition)
        => When(property, condition);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WhenAsync(Func<T, CancellationToken, Task<bool>> condition)
        => WhenAsync(condition);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WhenAsync<TProp>(Expression<Func<T, TProp>> property, Func<T, CancellationToken, Task<bool>> condition)
        => WhenAsync(property, condition);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.And<TMember>(Expression<Func<T, TMember>> property, Predicate<T> condition)
        => And(property, condition);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.Except<TMember>(Expression<Func<T, TMember>> property)
        => Except(property);
    IFluentTypeValidator<T> IFluentTypeValidator<T>.AlwaysValidate<TMember>(Expression<Func<T, TMember>> property)
        => AlwaysValidate(property);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WithMessage(string message) => WithMessage(message);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WithKey(string key) => WithKey(key);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.Localized(string resourceKey)
        => Localized(resourceKey);

    IFluentTypeValidator<TNext> IFluentTypeValidator<T>.For<TNext>() => For<TNext>();

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WithValidationResource<TResource>()
        => WithValidationResource<TResource>();

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WithValidationResource(Type? resourceType)
        => WithValidationResource(resourceType);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.WithCulture(CultureInfo culture)
        => WithCulture(culture);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.DisableConventionalKeys()
        => DisableConventionalKeys();

    IFluentTypeValidator<T> IFluentTypeValidator<T>.DisableConfigurationEnforcement(bool disableConfigurationEnforcement)
        => DisableConfigurationEnforcement(disableConfigurationEnforcement);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.UseFallbackMessage(string fallbackMessage)
        => UseFallbackMessage(fallbackMessage);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.Rule<TMember>(Expression<Func<T, TMember>> member)
        => Rule(member);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, RuleDefinitionBehavior behavior)
        => Rule(member, behavior);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must)
        => Rule(member, must);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must, RuleDefinitionBehavior behavior)
        => Rule(member, must, behavior);

    //ICollectionRuleBuilder<T, TElement> IFluentTypeValidator<T>.RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression)
    //    => RuleForEach(expression);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.RemovePendingRules(MemberInfo memberInfo)
        => RemovePendingRules(memberInfo);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.RemovePendingRules(MemberInfo memberInfo, Type validationAttributeType)
        => RemovePendingRules(memberInfo, validationAttributeType);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.RemoveRulesFor<TMember>(Expression<Func<T, TMember>> memberExpression)
        => RemoveRulesFor(memberExpression);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.RemoveRulesFor<TMember, TAttribute>(Expression<Func<T, TMember>> memberExpression)
        => RemoveRulesFor<TMember, TAttribute>(memberExpression);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.RemoveRulesFor<TMember>(Expression<Func<T, TMember>> memberExpression, Type attributeType)
        => RemoveRulesFor(memberExpression, attributeType);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.RemoveRulesExceptFor<TMember>(Expression<Func<T, TMember>> memberExpression)
        => RemoveRulesExceptFor(memberExpression);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.ClearRules()
        => ClearRules();

    IFluentTypeValidator<T> IFluentTypeValidator<T>.BeforeValidation(PreValidationValueProviderDelegate<T> configure)
        => BeforeValidation(configure);

    IFluentTypeValidator<T> IFluentTypeValidator<T>.AttachAttribute(ValidationAttribute attribute, Predicate<T>? when)
        => AddValidator(attribute, when);

    #endregion

    /// <summary>
    /// Makes sure the specified expression has at least one concrete 
    /// rule (with one or more validation attributes) registered.
    /// </summary>
    /// <param name="memberExpression">
    /// A member expression that contains a property, field, or method info.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// There is no rule for the specified <paramref name="memberExpression"/>.
    /// </exception>
    protected virtual void EnsureContainsAnyRule(Expression memberExpression)
    {
        if (_disableConfigurationEnforcement ?? GlobalRegistry.Default.ConfigurationEnforcementDisabled) return;

        MemberInfo memberInfo;
        if (!Registry.ContainsAny<T>(typeof(T), memberInfo = memberExpression.GetMemberInfo(), rule => rule.HasValidator))
            throw new InvalidOperationException($"There is no rule for the {memberInfo.Name} {memberInfo.MemberType}.");
    }

    /// <summary>
    /// Ensures that a pre-validation value provider delegate is not assigned more than once for a given member.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> of the member being configured.</param>
    /// <param name="providerDelegate">The current delegate being checked.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a pre-validation value provider delegate has already been assigned to the specified member,
    /// either in a pending rule or within an existing validation rule builder.
    /// </exception>
    protected virtual void EnsureSinglePreValidationValueProvider(MemberInfo member, PreValidationValueProviderDelegate? providerDelegate)
    {
        // Check for pre-validation delegates in the pending rules
        var pendingRuleMembers = _pendingRules
            .Where(r => r.ConfigureBeforeValidation != null && r.ConfigureBeforeValidation != providerDelegate)
            .Select(r => r.Expression.GetMemberInfo());

        // Check for pre-validation delegates in the fluent rule builders
        var builderRuleMembers = _validationRuleBuilders
            .SelectMany(builder => builder.GetRules())
            .Where(r => r.ConfigureBeforeValidation != null && r.ConfigureBeforeValidation != providerDelegate)
            .Select(r => r.Member);

        // Check for pre-validation delegates in the central rule registry within ValidationRuleGroupRegistry
        var registryMembers = Registry.GetRegistryForMember(typeof(T), member)
            .SelectMany(kvp => kvp.Value)
            .SelectMany(v => v.Rules)
            .Where(r => r.ConfigureBeforeValidation != null && r.ConfigureBeforeValidation != providerDelegate)
            .Select(rule => rule.Member);

        // Combine all member lists and check for an existing delegate on the current member.
        if (pendingRuleMembers.Union(builderRuleMembers).Union(registryMembers).Any(m => m.AreSameMembers(member)))
        {
            ThrowPreValidationError(member.Name);
        }
    }

    /// <summary>
    /// Adds the specified validation attribute to the collection of <see cref="PendingRule{T}.Attributes"/>
    /// and optionally sets a predicate that determines when the rule should be applied.
    /// </summary>
    /// <param name="attribute">The attribute to add to the collection of <see cref="PendingRule{T}.Attributes"/>.</param>
    /// <param name="when">A function that determines when the rule should be applied.</param>
    /// <returns>The current configurator for further chaining.</returns>
    /// <exception cref="InvalidOperationException">There's no pending rule to attach the attribute to.</exception>
    public virtual FluentTypeValidator<T> AddValidator(ValidationAttribute attribute, Predicate<T>? when = null)
    {
        if (_currentRule is null)
            throw new InvalidOperationException($"No pending rule to attach the attribute {attribute.GetType().Name} to.");

        var runtimeType = attribute.GetType();

        if (_currentRule.Attributes.Any(a => a.GetType() == runtimeType))
        {
            // check if the attribute supports multiple instance on the same member
            var usage = runtimeType.GetCustomAttribute<AttributeUsageAttribute>();

            if (usage != null && !usage.AllowMultiple)
            {
#if DEBUG
                Debug.WriteLine($"The current rule already contains an attribute of the same type.");
#endif
                return this;
            }
        }

        _currentRule.Attributes.Add(attribute);

        if (when is not null)
            _currentRule.Condition = model => when(model);

        return this;
    }

    /// <summary>
    /// This method is intended for internal use only and is exposed to the test project
    /// via the <c>InternalsVisibleTo</c> attribute for unit testing purposes.
    /// </summary>
    /// <param name="pendingRule">The <see cref="PendingRule{T}"/> to set as the current rule.</param>
    protected internal virtual void SetCurrentRule(PendingRule<T> pendingRule)
    {
        CommitCurrentRule();
        _currentRule = pendingRule;
    }

    /// <summary>
    /// Returns the <see cref="PendingRule{T}"/> being currently configured.
    /// This method is intended for internal use only and is exposed to the test project
    /// via the <c>InternalsVisibleTo</c> attribute for unit testing purposes.
    /// </summary>
    /// <returns></returns>
    protected internal virtual PendingRule<T>? GetCurrentRule() => _currentRule;

    /// <summary>
    /// Adds the currently pending rule to the rules to configure, and 
    /// dereferences it (by setting its value to <see langword="null"/>).
    /// </summary>
    protected virtual void CommitCurrentRule()
    {
        if (_currentRule is not null)
        {
            _pendingRules.Add(_currentRule);
            _currentRule = null;
        }
    }

    /// <summary>
    /// Adds the rules gathered by the <see cref="RuleFor{TMember}(Expression{Func{T, TMember}})"/>
    /// and <see cref="RuleForEach{TElement}(Expression{Func{T, IEnumerable{TElement}}})"/> methods.
    /// </summary>
    /// <param name="rules">The receving list.</param>
    protected virtual void AddRuleBuilders(List<IValidationRule> rules)
    {
        foreach (var builder in _validationRuleBuilders)
        {
            rules.AddRange(builder.GetRules());
        }
    }

    /// <summary>
    /// Performs consistency checks before adding the specified rules to the <see cref="ValidationRuleGroupRegistry"/>.
    /// </summary>
    /// <param name="rules">The rules to check and register.</param>
    protected virtual void RegisterRules(List<IValidationRule> rules)
    {
        // Group rules by type and ensure we don't include member's without reflected or declaring type.
        var typeGroups = rules.GroupBy(r => (r.Member.ReflectedType ?? r.Member.DeclaringType)!)
            .Where(r => r.Key != null);

        foreach (var validationRules in typeGroups)
        {
            var type = validationRules.Key;

            // Each group corresponds to a specific type.
            ValidationRuleGroupList group = new(type);

            // Group the rules by member to optimize merge operations within the registry.
            foreach (var memberRules in validationRules.GroupBy(r => r.Member))
            {
                List<IValidationRule> memberRuleList = [];
                foreach (var rule in memberRules)
                {
                    // Check for duplicate pre-validation delegates using the dedicated method.
                    if (rule.ConfigureBeforeValidation != null)
                    {
                        EnsureSinglePreValidationValueProvider(rule.Member, rule.ConfigureBeforeValidation);
                    }
                    memberRuleList.Add(rule);
                }
                group.Add(new ValidationRuleGroup(type, memberRules.Key, memberRuleList));
            }

            // Register the rules for the current type
            Registry.AddRules(type, group);
        }
    }

    /// <summary>
    /// Attempts to assign the <see cref="FluentTypeValidatorBase.Culture"/>'s
    /// value to the corresponding property on the specified resource type.
    /// </summary>
    /// <param name="type">The resource type for which to set the culture.</param>
    protected virtual void AssignCultureTo(Type? type)
    {
        var oldType = ValidationResourceType;
        ValidationResourceType = type;
        (type ?? oldType).TrySetResourceManagerCulture(Culture, fallbackToType: true);
    }

    #region helpers

    private void MarkAsUnbuilt()
    {
        // Any new configuration for the type marks it as not built.
        Registry.MarkBuilt(typeof(T), false);
    }

    [DoesNotReturn]
    private static void ThrowPreValidationError(string memberName)
    {
        throw new InvalidOperationException(
                "A pre-validation value provider delegate can only be assigned once per member " +
                $"({memberName}) on type '{typeof(T).Name}'.");
    }

    #endregion
}
