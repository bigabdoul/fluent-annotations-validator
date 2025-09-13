using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Provides a fluent, type-safe configuration surface for defining validation logic
/// on a specific model type using <see cref="ValidationBehaviorOptions"/>.
/// </summary>
/// <typeparam name="T">The model or DTO type being configured.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationTypeConfigurator{T}"/> class.
/// </remarks>
/// <param name="root">
/// The root-level validation configurator allowing to transition 
/// to another <see cref="ValidationTypeConfigurator{T}"/>.
/// </param>
/// <param name="options">An object used to configure fluent validations behavior.</param>
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
[Obsolete("Will be removed before releasing. Use FluentAnnotationsValidator.Configuration.FluentValidatorType<T>", true)]
public class ValidationTypeConfigurator<T>(ValidationConfigurator root, ValidationBehaviorOptions options)
    : ValidationTypeConfiguratorBase(typeof(T)), IValidationTypeConfigurator<T>
{
    private static readonly Predicate<T> AlwaysTruePredicate = _ => true;
    private readonly HashSet<PendingRule<T>> _pendingRules = [];
    private readonly List<IValidationRuleBuilder> _validationRuleBuilders = [];
    private readonly ValidationConfigurator root = root;
    private readonly ValidationBehaviorOptions options = options;

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
    /// Gets the validation behavior options.
    /// </summary>
    public ValidationBehaviorOptions Options => options;

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithValidationResource{TResource}()"/>
    public virtual ValidationTypeConfigurator<T> WithValidationResource<TResource>()
    {
        AssignCultureTo(typeof(TResource));
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithValidationResource(Type?)"/>
    public virtual ValidationTypeConfigurator<T> WithValidationResource(Type? resourceType)
    {
        AssignCultureTo(resourceType);
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithCulture(CultureInfo)"/>
    public virtual ValidationTypeConfigurator<T> WithCulture(CultureInfo culture)
    {
        Culture = culture;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.For{TNext}"/>
    public ValidationTypeConfigurator<TNext> For<TNext>()
    {
        //CommitCurrentRule();
        //GlobalValidationRegistry.Default.Register(typeof(T), this);
        //return root.For<TNext>();
        throw new NotSupportedException("This class is obsolete and will be removed in the final release. " +
            $"Use the {typeof(FluentTypeValidatorRoot).Name} class.");
    }

    #region Rules Management

    /// <summary>
    /// Determines whether the current configurator contains 
    /// neither pending rules nor validation rule builders.
    /// </summary>
    protected virtual bool IsEmpty => _pendingRules.Count == 0 && _validationRuleBuilders.Count == 0;

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Rule{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> member)
        => Rule(member, RuleDefinitionBehavior.Replace);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Rule{TMember}(Expression{Func{T, TMember}}, RuleDefinitionBehavior)"/>
    public virtual ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> member, RuleDefinitionBehavior behavior)
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

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Rule{TMember}(Expression{Func{T, TMember}}, Predicate{TMember})"/>
    public virtual ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must)
        => Rule(member, must, RuleDefinitionBehavior.Replace);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Rule{TMember}(Expression{Func{T, TMember}}, Predicate{TMember}, RuleDefinitionBehavior)"/>
    public virtual ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must, RuleDefinitionBehavior behavior)
        => Rule(member, behavior).AddValidator(new MustAttribute<TMember>(must), AlwaysTruePredicate);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RuleFor{TMember}(Expression{Func{T, TMember}})"/>
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

        var configurator = new ValidationRuleBuilder<T, TMember>(newPendingRule);
        _validationRuleBuilders.Add(configurator);

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
        var configurator = new ValidationRuleBuilder<T, TElement>(newPendingRule);
        _validationRuleBuilders.Add(configurator);

        return configurator;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemoveRulesFor{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> member)
    {
        var memberInfo = member.GetMemberInfo();
        _ = Options.RemoveAll(memberInfo);
        return RemovePendingRules(memberInfo);
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemoveRulesFor{TMember, TAttribute}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> RemoveRulesFor<TMember, TAttribute>(Expression<Func<T, TMember>> member)
        where TAttribute : ValidationAttribute
    {
        var memberInfo = member.GetMemberInfo();

        Options.RemoveAll<TAttribute>((member, attribute) => memberInfo.AreSameMembers(member));
        RemovePendingRules(memberInfo, typeof(TAttribute));

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemoveRulesFor{TMember}(Expression{Func{T, TMember}}, Type)"/>
    public virtual ValidationTypeConfigurator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> member, Type attributeType)
    {
        var memberInfo = member.GetMemberInfo();
        _ = Options.RemoveAll(memberInfo, attributeType);
        return RemovePendingRules(memberInfo, attributeType);
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemoveRulesExceptFor{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> RemoveRulesExceptFor<TMember>(Expression<Func<T, TMember>> member)
    {
        var memberInfo = member.GetMemberInfo();

        _ = Options.RemoveAll(mi => !memberInfo.AreSameMembers(mi));
        _ = _pendingRules.RemoveWhere(rule => !memberInfo.AreSameMembers(rule.Expression.GetMemberInfo()));
        _ = _validationRuleBuilders.RemoveAll(builder => !memberInfo.AreSameMembers(builder.Member.GetMemberInfo()));

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemovePendingRules(MemberInfo)"/>
    public virtual ValidationTypeConfigurator<T> RemovePendingRules(MemberInfo memberInfo)
    {
        _ = _pendingRules.RemoveWhere(rule => memberInfo.AreSameMembers(rule.Expression.GetMemberInfo()));
        _ = _validationRuleBuilders.RemoveAll(builder => memberInfo.AreSameMembers(builder.Member.GetMemberInfo()));
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemovePendingRules(MemberInfo, Type)"/>
    public virtual ValidationTypeConfigurator<T> RemovePendingRules(MemberInfo memberInfo, Type validationAttributeType)
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
    /// <returns>The current <see cref="ValidationTypeConfigurator{T}"/> instance for fluent chaining.</returns>
    public virtual ValidationTypeConfigurator<T> ClearRules()
    {
        _pendingRules.Clear();
        _validationRuleBuilders.Clear();
        Options.RemoveAllForType(typeof(T));
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
    public virtual ValidationTypeConfigurator<T> When(Predicate<T> condition)
    {
        // Applies to the predicate of a rule created with .Rule(...);
        // throws if _currentRule is not defined

        if (_currentRule is null)
            throw new InvalidOperationException("You must create a rule with the .Rule(...) method.");

        // .Rule(...) are attribute-based; therefore, the predicate
        // should only be applicable to the attribute being configured

        if (_currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(_currentRule.Expression);

        _currentRule.Condition = model => condition((T)model);

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.When{TMember}(Expression{Func{T, TMember}}, Predicate{T})"/>
    public virtual ValidationTypeConfigurator<T> When<TMember>(Expression<Func<T, TMember>> member, Predicate<T> condition)
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
        _currentRule.Condition = model => condition((T)model); // override the predicate, or compose?

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WhenAsync(Func{T, CancellationToken, Task{bool}})"/>
    public virtual ValidationTypeConfigurator<T> WhenAsync(Func<T, CancellationToken, Task<bool>> condition)
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

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WhenAsync{TProp}(Expression{Func{T, TProp}}, Func{T, CancellationToken, Task{bool}})"/>
    public virtual ValidationTypeConfigurator<T> WhenAsync<TProp>(Expression<Func<T, TProp>> member, Func<T, CancellationToken, Task<bool>> condition)
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

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.And{TMember}(Expression{Func{T, TMember}}, Predicate{T})"/>
    public virtual ValidationTypeConfigurator<T> And<TMember>(Expression<Func<T, TMember>> property, Predicate<T> condition)
        => When(property, condition);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Except{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> Except<TMember>(Expression<Func<T, TMember>> property)
    {
        CommitCurrentRule();
        return RemoveRulesFor(property);
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.AlwaysValidate{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> AlwaysValidate<TMember>(Expression<Func<T, TMember>> property)
        => When(property, AlwaysTruePredicate);

    #endregion

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithMessage(string)"/>
    public virtual ValidationTypeConfigurator<T> WithMessage(string message)
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

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithKey(string)"/>
    public virtual ValidationTypeConfigurator<T> WithKey(string key)
    {
        if (_currentRule is not null)
            _currentRule.Key = key;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Localized(string)"/>
    public virtual ValidationTypeConfigurator<T> Localized(string resourceKey)
    {
        if (_currentRule is not null)
            _currentRule.ResourceKey = resourceKey;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.DisableConventionalKeys"/>
    public virtual ValidationTypeConfigurator<T> DisableConventionalKeys()
    {
        _useConventionalKeys = false;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.DisableConfigurationEnforcement(bool)"/>
    public virtual ValidationTypeConfigurator<T> DisableConfigurationEnforcement(bool disableConfigurationEnforcement)
    {
        _disableConfigurationEnforcement = disableConfigurationEnforcement;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.UseFallbackMessage(string)"/>
    public virtual ValidationTypeConfigurator<T> UseFallbackMessage(string fallbackMessage)
    {
        _fallbackMessage = fallbackMessage;
        if (_currentRule is not null)
            _currentRule.FallbackMessage = _fallbackMessage;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.BeforeValidation(PreValidationValueProviderDelegate{T})"/>
    public virtual ValidationTypeConfigurator<T> BeforeValidation(PreValidationValueProviderDelegate<T> configure)
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

        if (IsEmpty)
        {
            return RulesFromLastBuild.AsReadOnly();
        }

        // A single, unified list to collect all rules before registration
        var rulesToRegister = new List<IValidationRule>();

        // Step 1: Process and transform rules from pending rule collection
        foreach (var pendingRule in _pendingRules)
        {
            var member = pendingRule.Expression.GetMemberInfo();

            if (pendingRule.Attributes.Count > 0)
            {
                foreach (var attr in pendingRule.Attributes)
                {
                    var newRule = pendingRule.CreateRuleFromPending(member, attr);
                    newRule.ConfigureBeforeValidation = pendingRule.ConfigureBeforeValidation;
                    rulesToRegister.Add(newRule);
                }
            }
            else
            {
                var newRule = pendingRule.CreateRuleFromPending(member);
                newRule.ConfigureBeforeValidation = pendingRule.ConfigureBeforeValidation;
                rulesToRegister.Add(newRule);
            }
        }

        // Step 2: Add rules from the rule builders
        AddRuleBuilders(rulesToRegister);

        // Step 3: Register all rules, performing consistency checks
        RegisterRules(rulesToRegister);

        // Clear the temporary collections
        _pendingRules.Clear();
        _validationRuleBuilders.Clear();

        RulesFromLastBuild.Clear();
        RulesFromLastBuild.AddRange(rulesToRegister);

        return rulesToRegister;
    }

    /// <inheritdoc/>
    public void DiscardRulesFromLastBuild() => RulesFromLastBuild?.Clear();

    #region IValidationTypeConfigurator<T>

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.When(Predicate<T> condition)
        => When(condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.When<TMember>(Expression<Func<T, TMember>> property, Predicate<T> condition)
        => When(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WhenAsync(Func<T, CancellationToken, Task<bool>> condition)
        => WhenAsync(condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WhenAsync<TProp>(Expression<Func<T, TProp>> property, Func<T, CancellationToken, Task<bool>> condition)
        => WhenAsync(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.And<TMember>(Expression<Func<T, TMember>> property, Predicate<T> condition)
        => And(property, condition);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Except<TMember>(Expression<Func<T, TMember>> property)
        => Except(property);
    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.AlwaysValidate<TMember>(Expression<Func<T, TMember>> property)
        => AlwaysValidate(property);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WithMessage(string message) => WithMessage(message);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WithKey(string key) => WithKey(key);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Localized(string resourceKey)
        => Localized(resourceKey);

    IValidationTypeConfigurator<TNext> IValidationTypeConfigurator<T>.For<TNext>() => For<TNext>();

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WithValidationResource<TResource>()
        => WithValidationResource<TResource>();

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WithValidationResource(Type? resourceType)
        => WithValidationResource(resourceType);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.WithCulture(CultureInfo culture)
        => WithCulture(culture);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.DisableConventionalKeys()
        => DisableConventionalKeys();

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.DisableConfigurationEnforcement(bool disableConfigurationEnforcement)
        => DisableConfigurationEnforcement(disableConfigurationEnforcement);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.UseFallbackMessage(string fallbackMessage)
        => UseFallbackMessage(fallbackMessage);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Rule<TMember>(Expression<Func<T, TMember>> member)
        => Rule(member);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, RuleDefinitionBehavior behavior)
        => Rule(member, behavior);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must)
        => Rule(member, must);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, Predicate<TMember> must, RuleDefinitionBehavior behavior)
        => Rule(member, must, behavior);

    //ICollectionRuleBuilder<T, TElement> IValidationTypeConfigurator<T>.RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression)
    //    => RuleForEach(expression);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.RemovePendingRules(MemberInfo memberInfo)
        => RemovePendingRules(memberInfo);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.RemovePendingRules(MemberInfo memberInfo, Type validationAttributeType)
        => RemovePendingRules(memberInfo, validationAttributeType);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.RemoveRulesFor<TMember>(Expression<Func<T, TMember>> memberExpression)
        => RemoveRulesFor(memberExpression);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.RemoveRulesFor<TMember, TAttribute>(Expression<Func<T, TMember>> memberExpression)
        => RemoveRulesFor<TMember, TAttribute>(memberExpression);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.RemoveRulesFor<TMember>(Expression<Func<T, TMember>> memberExpression, Type attributeType)
        => RemoveRulesFor(memberExpression, attributeType);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.RemoveRulesExceptFor<TMember>(Expression<Func<T, TMember>> memberExpression)
        => RemoveRulesExceptFor(memberExpression);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.ClearRules()
        => ClearRules();

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.BeforeValidation(PreValidationValueProviderDelegate<T> configure)
        => BeforeValidation(configure);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.AttachAttribute(ValidationAttribute attribute, Predicate<T>? when)
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
        if (_disableConfigurationEnforcement ?? Options.ConfigurationEnforcementDisabled) return;

        MemberInfo memberInfo;
        if (!Options.ContainsAny<T>(memberInfo = memberExpression.GetMemberInfo(), rule => rule.HasValidator))
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

        // Check for pre-validation delegates in the central rule registry within ValidationBehaviorOptions
        var registryMembers = Options.GetRegistryForMember(member)
            .SelectMany(kvp => kvp.Value)
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
    public virtual ValidationTypeConfigurator<T> AddValidator(ValidationAttribute attribute, Predicate<T>? when = null)
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
            _currentRule.Condition = model => when((T)model);

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
    /// Performs consistency checks before adding the specified rules to the <see cref="ValidationBehaviorOptions"/>.
    /// </summary>
    /// <param name="rules">The rules to check and register.</param>
    protected virtual void RegisterRules(List<IValidationRule> rules)
    {
        foreach (var rule in rules)
        {
            // Check for duplicate pre-validation delegates using the dedicated method.
            if (rule.ConfigureBeforeValidation != null)
            {
                EnsureSinglePreValidationValueProvider(rule.Member, rule.ConfigureBeforeValidation);
            }

            // Add rule to the main registry
            Options.AddRule(rule.Member, rule);
        }
    }

    /// <summary>
    /// Attempts to assign the <see cref="ValidationTypeConfiguratorBase.Culture"/>'s
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

    private static void ThrowPreValidationError(string memberName)
    {
        throw new InvalidOperationException(
                "A pre-validation value provider delegate can only be assigned once per member " +
                $"({memberName}) on type '{typeof(T).Name}'.");
    }

    IValidationRuleBuilder? _parentRuleBuilder;

    internal void SetParent<TProp>(IValidationRuleBuilder<TProp, T> builder)
    {
        _parentRuleBuilder = builder;
    }

    internal IValidationRuleBuilder? GetParent() => _parentRuleBuilder;

    internal IValidationRuleBuilder<TProp, T>? GetParent<TProp>()
        => (ValidationRuleBuilder<TProp, T>?)_parentRuleBuilder;

    #endregion
}
