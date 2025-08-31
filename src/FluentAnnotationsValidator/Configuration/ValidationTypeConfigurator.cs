using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Provides a fluent, type-safe configuration surface for defining conditional validation logic
/// on a specific model type using <see cref="ValidationBehaviorOptions"/>.
/// </summary>
/// <typeparam name="T">The model or DTO type being configured.</typeparam>
/// <param name="parent">The parent validation configurator.</param>
/// <param name="options">An object used to configure fluent validations behavior.</param>
/// <remarks>
/// This configurator allows chaining multiple conditions and metadata overrides such as custom messages,
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
public class ValidationTypeConfigurator<T>(ValidationConfigurator parent, ValidationBehaviorOptions options)
    : ValidationTypeConfiguratorBase(typeof(T)), IValidationTypeConfigurator<T>
{
    private static readonly Func<T, bool> TruePredicate = _ => true;
    private static readonly Func<T, bool> DefaultAttributePredicate = _ => true;

    private readonly HashSet<PendingRule<T>> _pendingRules = [];
    private readonly List<IValidationRuleBuilder> _validationRuleBuilders = [];

    private PendingRule<T>? _currentRule;
    private bool _useConventionalKeys = true;
    private string? _fallbackMessage;
    private bool? _disableConfigurationEnforcement;

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
        CommitCurrentRule();
        ValidationConfiguratorStore.Registry.Register(typeof(T), this);
        return parent.For<TNext>();
    }

    #region Rules Management

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
            predicate: DefaultAttributePredicate, // Always validate unless overridden by .When(...)
            resourceType: ValidationResourceType,
            culture: Culture,
            fallbackMessage: _fallbackMessage,
            useConventionalKeys: _useConventionalKeys
        );

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Rule{TMember}(Expression{Func{T, TMember}}, Func{TMember, bool})"/>
    public virtual ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Func<TMember, bool> must)
        => Rule(member, must, RuleDefinitionBehavior.Replace);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Rule{TMember}(Expression{Func{T, TMember}}, Func{TMember, bool}, RuleDefinitionBehavior)"/>
    public virtual ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> member, Func<TMember, bool> must, RuleDefinitionBehavior behavior)
        => Rule(member, behavior).AttachAttribute(new MustAttribute<TMember>(must), DefaultAttributePredicate);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RuleFor{TMember}(Expression{Func{T, TMember}})"/>
    public IValidationRuleBuilder<T, TMember> RuleFor<TMember>(Expression<Func<T, TMember>> member)
    {
        CommitCurrentRule();

        var rule = new PendingRule<T>(
            member,
            predicate: DefaultAttributePredicate, // Always validate unless overridden by .When(...)
            resourceType: ValidationResourceType,
            culture: Culture,
            fallbackMessage: _fallbackMessage,
            useConventionalKeys: _useConventionalKeys
        );

        var configurator = new ValidationRuleBuilder<T, TMember>(rule);
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
        _ = _pendingRules.RemoveWhere(rule => !memberInfo.AreSameMembers(rule.MemberExpression.GetMemberInfo()));
        _ = _validationRuleBuilders.RemoveAll(builder => !memberInfo.AreSameMembers(builder.Member.GetMemberInfo()));

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemovePendingRules(MemberInfo)"/>
    public virtual ValidationTypeConfigurator<T> RemovePendingRules(MemberInfo memberInfo)
    {
        _ = _pendingRules.RemoveWhere(rule => memberInfo.AreSameMembers(rule.MemberExpression.GetMemberInfo()));
        _ = _validationRuleBuilders.RemoveAll(builder => memberInfo.AreSameMembers(builder.Member.GetMemberInfo()));
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.RemovePendingRules(MemberInfo, Type)"/>
    public virtual ValidationTypeConfigurator<T> RemovePendingRules(MemberInfo memberInfo, Type validationAttributeType)
    {
        ArgumentNullException.ThrowIfNull(validationAttributeType);

        var attributesRemoved = 0;

        foreach (var rule in _pendingRules)
        {
            if (!memberInfo.AreSameMembers(rule.MemberExpression.GetMemberInfo()))
                continue;

#if DEBUG
            var aspect = rule.ToString();
#endif
            // Remove attributes of the specified type
            var count = rule.Attributes.RemoveAll(attr =>
                EqualityComparer<Type>.Default.Equals(attr.GetType(), validationAttributeType));

            attributesRemoved += count;
#if DEBUG
            Debug.WriteLine($"Removed {count} attribute(s) from pending rule: {aspect}.");
#endif
        }

        foreach (var builder in _validationRuleBuilders)
        {
            if (!memberInfo.AreSameMembers(builder.Member.GetMemberInfo()))
                continue;

            var count = builder.RemoveRules(rule =>
                rule.HasAttribute &&
                EqualityComparer<Type>.Default.Equals(rule.Attribute!.GetType(), validationAttributeType));

            attributesRemoved += count;
            Debug.WriteLine($"Removed {count} attribute(s) from validation rule builders.");
        }

        Debug.WriteLine($"Removed a total of {attributesRemoved} attribute(s) from all pending rules.");

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
    /// Applies to the predicate of a rule created with <see cref="Rule{TMember}(Expression{Func{T, TMember}})"/>.
    /// </summary>
    /// <param name="condition">A function that evaluates when the rule is applied.</param>
    /// <returns>The current configurator for further chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// You must create a rule with the <see cref="Rule{TMember}(Expression{Func{T, TMember}})"/> method.
    /// </exception>
    public virtual ValidationTypeConfigurator<T> When(Func<T, bool> condition)
    {
        // Applies to the predicate of a rule created with .Rule(...);
        // throws if _currentRule is not defined

        if (_currentRule is null)
            throw new InvalidOperationException("You must create a rule with the .Rule(...) method.");

        // .Rule(...) are attribute-based; therefore, the predicate
        // should only be applicable to the attribute being configured

        if (_currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(_currentRule.MemberExpression);

        _currentRule.Predicate = condition;

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.When{TMember}(Expression{Func{T, TMember}}, Func{T, bool})"/>
    public virtual ValidationTypeConfigurator<T> When<TMember>(Expression<Func<T, TMember>> member, Func<T, bool> condition)
    {
        if (_currentRule is null || _currentRule.Attributes.Count == 0)
            EnsureContainsAnyRule(member);

        MemberInfo memberInfo;

        if (_currentRule is null || !_currentRule.MemberExpression.GetMemberInfo().AreSameMembers(memberInfo = member.GetMemberInfo()))
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
        _currentRule.Predicate = condition; // override the predicate, or compose?

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.And{TMember}(Expression{Func{T, TMember}}, Func{T, bool})"/>
    public virtual ValidationTypeConfigurator<T> And<TMember>(Expression<Func<T, TMember>> property, Func<T, bool> condition)
        => When(property, condition);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Except{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> Except<TMember>(Expression<Func<T, TMember>> property)
    {
        CommitCurrentRule();
        return RemoveRulesFor(property);
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.AlwaysValidate{TMember}(Expression{Func{T, TMember}})"/>
    public virtual ValidationTypeConfigurator<T> AlwaysValidate<TMember>(Expression<Func<T, TMember>> property)
        => When(property, TruePredicate);

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

        EnsureSinglePreValidationValueProvider(_currentRule.MemberExpression.GetMemberInfo());

        _currentRule.ConfigureBeforeValidation = (instance, member, memberValue) =>
            configure.Invoke((T)instance, member, memberValue);

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Build"/>
    public virtual void Build()
    {
        CommitCurrentRule();

        foreach (var rule in _pendingRules)
        {
            var member = rule.MemberExpression.GetMemberInfo();

            rule.ResourceType ??= Options.SharedResourceType;
            rule.Culture ??= Options.SharedCulture;
            rule.UseConventionalKeys ??= Options.UseConventionalKeys;

            if (rule.Attributes.Count > 0)
            {
                foreach (var attr in rule.Attributes)
                {
                    var newRule = rule.CreateRuleFromPending(member, attr, rule.Predicate);
                    newRule.ConfigureBeforeValidation = rule.ConfigureBeforeValidation;
                    Options.AddRule(member, newRule);
                }
            }
            else
            {
                parent.Register(opts =>
                {
                    var newRule = rule.CreateRuleFromPending(member);
                    newRule.ConfigureBeforeValidation = rule.ConfigureBeforeValidation;
                    opts.AddRule(member, newRule);
                });
            }
        }

        foreach (var builder in _validationRuleBuilders)
        {
            foreach (var rule in builder.GetRules())
            {
                Options.AddRule(rule.Member, rule);
            }
        }

        _pendingRules.Clear();
        _validationRuleBuilders.Clear();

        parent.Build();
    }

    #region IValidationTypeConfigurator<T>

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.When<TMember>(Expression<Func<T, TMember>> property, Func<T, bool> condition)
        => When(property, condition);
    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.And<TMember>(Expression<Func<T, TMember>> property, Func<T, bool> condition)
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

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, Func<TMember, bool> must)
        => Rule(member, must);

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Rule<TMember>(Expression<Func<T, TMember>> member, Func<TMember, bool> must, RuleDefinitionBehavior behavior)
        => Rule(member, must, behavior);

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
        if (!Options.ContainsAny<T>(memberInfo = memberExpression.GetMemberInfo(), rule => rule.HasAttribute))
            throw new InvalidOperationException($"There is no rule for the {memberInfo.Name} {memberInfo.MemberType}.");
    }

    /// <summary>
    /// Ensures that a pre-validation value provider delegate is not assigned more than once for a given member.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> of the member being configured.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a pre-validation value provider delegate has already been assigned to the specified member,
    /// either in a pending rule or within an existing validation rule builder.
    /// </exception>
    protected virtual void EnsureSinglePreValidationValueProvider(MemberInfo member)
    {
        var pendingRuleMembers = _pendingRules
            .Where(r => r.ConfigureBeforeValidation != null)
            .Select(r => r.MemberExpression.GetMemberInfo());

        var builderRuleMembers = _validationRuleBuilders
            .SelectMany(builder => builder.GetRules())
            .Where(r => r.ConfigureBeforeValidation != null)
            .Select(r => r.Member);

        if (pendingRuleMembers.Union(builderRuleMembers).Any(m => m.AreSameMembers(member)))
        {
            throw new InvalidOperationException(
                "The pre-validation value provider delegate cannot be assigned more than once to any rule for the type " +
                $"({typeof(T).Name}) and member ({member.Name}) being configured.");
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
    protected internal ValidationTypeConfigurator<T> AttachAttribute(ValidationAttribute attribute, Func<T, bool>? when = null)
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
                //var mi = _currentRule.Member.GetMemberInfo();
                Debug.WriteLine($"The current rule already contains an attribute of the same type.");
                return this;
            }
        }

        _currentRule.Attributes.Add(attribute);

        if (when is not null)
            _currentRule.Predicate = when;

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

    #region helpers

    private void AssignCultureTo(Type? type)
    {
        var oldType = ValidationResourceType;
        ValidationResourceType = type;
        (type ?? oldType).TrySetResourceManagerCulture(Culture, fallbackToType: true);
    }

    #endregion
}
