using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
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
/// Typical usage:
/// <code>
/// services.UseFluentAnnotations()
///     .For&lt;LoginDto&gt;()
///         .When(x =&gt; x.Email, dto =&gt; dto.Role == "Admin")
///             .WithMessage("Email is required for admins.")
///             .WithKey("Email.AdminRequired")
///             .Localized("Admin_Email_Required")
///         .AlwaysValidate(x =&gt; x.Password)
///         .Except(x =&gt; x.Role)
///     .Build();
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

    /// <summary>
    /// Gets the validation behavior options.
    /// </summary>
    public ValidationBehaviorOptions Options => options;

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithValidationResource{TResource}()"/>
    public ValidationTypeConfigurator<T> WithValidationResource<TResource>()
    {
        AssignCultureTo(typeof(TResource));
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithValidationResource(Type?)"/>
    public ValidationTypeConfigurator<T> WithValidationResource(Type? resourceType)
    {
        AssignCultureTo(resourceType);
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithCulture(CultureInfo)"/>
    public ValidationTypeConfigurator<T> WithCulture(CultureInfo culture)
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

    public ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> property)
        => Rule(property, RuleDefinitionBehavior.Replace);

    public ValidationTypeConfigurator<T> Rule<TMember>(Expression<Func<T, TMember>> property,
        RuleDefinitionBehavior behavior)
    {
        CommitCurrentRule();

        if (behavior == RuleDefinitionBehavior.Replace)
            RemoveRulesFor(property);

        _currentRule = new PendingRule<T>(
            member: property,
            predicate: DefaultAttributePredicate, // Always validate unless overridden by .When(...)
            resourceType: ValidationResourceType,
            culture: Culture,
            fallbackMessage: _fallbackMessage,
            useConventionalKeys: _useConventionalKeys
        );

        return this;
    }

    #region When

    public IValidationRuleBuilder<T, TMember> RuleFor<TMember>(Expression<Func<T, TMember>> property)
    {
        //Rule(property);
        CommitCurrentRule();

        var rule = new PendingRule<T>(
            member: property,
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

    /// <summary>
    /// Applies to the predicate of a rule created with <see cref="Rule{TMember}(Expression{Func{T, TMember}})"/>.
    /// </summary>
    /// <param name="condition"></param>
    /// <returns>The current configurator for further chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// You must create a rule with the <see cref="Rule{TMember}(Expression{Func{T, TMember}})"/> method.
    /// </exception>
    public ValidationTypeConfigurator<T> When(Func<T, bool> condition)
    {
        // Applies to the predicate of a rule created with .Rule(...);
        // throws if _currentRule is not defined

        if (_currentRule is null)
            throw new InvalidOperationException("You must create a rule with the .Rule(...) method.");

        // .Rule(...) are attribute-based; therefore, the predicate
        // should only be applicable to the attribute being configured
        //var container = _currentRule.Attributes.Last();
        //container.When = condition;
        _currentRule.Predicate = condition;

        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.When{TMember}(Expression{Func{T, TMember}}, Func{T, bool})"/>
    public ValidationTypeConfigurator<T> When<TMember>(Expression<Func<T, TMember>> property, Func<T, bool> condition)
    {
        // Should only commit and create a new rule if the current rule is NOT one initiated with Rule<TMember>(...);
        // in this case, the PendingRule.Attributes should have at least one attribute
        if (ShouldOverrideCurrentRule(property))
        {
            // continue configuration of the current rule
            _currentRule!.Predicate = condition; // override the predicate, or compose?
        }
        else
        {
            CommitCurrentRule();
            _currentRule = new PendingRule<T>(
                member: property,
                predicate: model => condition(model),
                resourceType: ValidationResourceType,
                culture: Culture,
                fallbackMessage: _fallbackMessage,
                useConventionalKeys: _useConventionalKeys
            );
        }
        return this;
    }

    #endregion

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.And{TMember}(Expression{Func{T, TMember}}, Func{T, bool})"/>
    public ValidationTypeConfigurator<T> And<TMember>(Expression<Func<T, TMember>> property, Func<T, bool> condition)
        => When(property, condition);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Except{TMember}(Expression{Func{T, TMember}})"/>
    public ValidationTypeConfigurator<T> Except<TMember>(Expression<Func<T, TMember>> property)
        => When(property, _ => false);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.AlwaysValidate{TMember}(Expression{Func{T, TMember}})"/>
    public ValidationTypeConfigurator<T> AlwaysValidate<TMember>(Expression<Func<T, TMember>> property)
        => When(property, TruePredicate);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithMessage(string)"/>
    public ValidationTypeConfigurator<T> WithMessage(string message)
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
    public ValidationTypeConfigurator<T> WithKey(string key)
    {
        if (_currentRule is not null)
            _currentRule.Key = key;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Localized(string)"/>
    public ValidationTypeConfigurator<T> Localized(string resourceKey)
    {
        if (_currentRule is not null)
            _currentRule.ResourceKey = resourceKey;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.DisableConventionalKeys"/>
    public ValidationTypeConfigurator<T> DisableConventionalKeys()
    {
        _useConventionalKeys = false;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.UseFallbackMessage(string)"/>
    public ValidationTypeConfigurator<T> UseFallbackMessage(string fallbackMessage)
    {
        _fallbackMessage = fallbackMessage;
        if (_currentRule is not null)
            _currentRule.FallbackMessage = _fallbackMessage;
        return this;
    }

    public ValidationTypeConfigurator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> property)
    {
        var member = property.GetMemberInfo();
        var removed = Options.RemoveAll(member);

        // clear any previous rule
        var count = _pendingRules.RemoveWhere(r => member.AreSameMembers(r.Member.GetMemberInfo()));
        Debug.WriteLine("Removed from ValidationBehaviorOptions: {0}\nRemoved from pending rules: {1}", removed, count);

        return this;
    }

    public ValidationTypeConfigurator<T> RemoveRulesFor<TMember, TAttribute>(Expression<Func<T, TMember>> property)
        where TAttribute : ValidationAttribute
    {
        var memberInfo = property.GetMemberInfo();
        Options.RemoveAll<TAttribute>((member, attribute) => EqualityComparer<MemberInfo>.Default.Equals(member, memberInfo));
        return this;
    }

    public ValidationTypeConfigurator<T> RemoveRulesFor<TMember>(Expression<Func<T, TMember>> property, Type attributeType)
    {
        var memberInfo = property.GetMemberInfo();
        Options.RemoveAll(memberInfo, attributeType);
        return this;
    }

    public ValidationTypeConfigurator<T> ClearRules()
    {
        _pendingRules.Clear();
        Options.RemoveAllForType(typeof(T));
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Build"/>
    public void Build()
    {
        CommitCurrentRule();

        foreach (var rule in _pendingRules)
        {
            var member = rule.Member.GetMemberInfo();

            rule.ResourceType ??= Options.CommonResourceType;
            rule.Culture ??= Options.CommonCulture;
            rule.UseConventionalKeys ??= Options.UseConventionalKeys;

            if (rule.Attributes.Count > 0)
            {
                foreach (var attr in rule.Attributes)
                {
                    var existingRules = Options.GetRules(member);
                    // each attribute has its own rule
                    var newRule = rule.CreateRuleFromPending(member, attr, rule.Predicate);
                    if (existingRules is null || !existingRules.Contains(newRule))
                        Options.AddRule(member, newRule);
                }
            }
            else
            {
                parent.Register(opts =>
                {
                    var existingRules = Options.GetRules(member);
                    var newRule = rule.CreateRuleFromPending(member);
                    if (existingRules is null || !existingRules.Contains(newRule))
                        opts.AddRule(member, newRule);
                });
            }
        }

        foreach (var builder in _validationRuleBuilders)
        {
            foreach (var rule in builder.GetRules())
            {
                //Options.AddRule(rule.Member, rule);
                var existingRules = Options.GetRules(rule.Member);
                if (existingRules is null || !existingRules.Contains(rule))
                    Options.AddRule(rule.Member, rule);
            }
        }

        _pendingRules.Clear();
        _validationRuleBuilders.Clear();

        parent.Build();
    }

    /// <summary>
    /// Deterines whether the current rule should be overridden.
    /// </summary>
    /// <typeparam name="TMember">The member type.</typeparam>
    /// <param name="member">The member (property or field) expression.</param>
    /// <returns>
    /// <see langword="true"/> if the rule should be overridden; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The current rule should be overridden when it's defined (not <see langword="null"/>),
    /// contains attributes (<see cref="PendingRule{T}.Attributes"/>.Count > 0), and its member
    /// name matches the specified <paramref name="member"/> name.
    /// </remarks>
    protected virtual bool ShouldOverrideCurrentRule<TMember>(Expression<Func<T, TMember>> member) => 
        _currentRule != null && 
        _currentRule.Attributes.Count != 0 && 
        _currentRule.Member.GetMemberInfo().AreSameMembers(member.GetMemberInfo());
    
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

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.UseFallbackMessage(string fallbackMessage)
        => UseFallbackMessage(fallbackMessage);

    #endregion

    protected internal ValidationTypeConfigurator<T> AttachAttribute(ValidationAttribute attribute, Func<T, bool>? when = null)
    {
        if (_currentRule is null)
            throw new InvalidOperationException($"No pending rule to attach the attribute {attribute.GetType().Name} to.");

        if (_currentRule.Attributes.Any(a => a.GetType() == attribute.GetType()))
        {
            //var mi = _currentRule.Member.GetMemberInfo();
            Debug.WriteLine($"The current rule already contains an attribute of the same type.");
            return this;
        }

        _currentRule.Attributes.Add(attribute);

        if (when is not null)
            _currentRule.Predicate = when;

        return this;
    }

    #region helpers

    private void CommitCurrentRule()
    {
        if (_currentRule is not null)
        {
            _pendingRules.Add(_currentRule);
            _currentRule = null;
        }
    }

    private void AssignCultureTo(Type? type)
    {
        var oldType = ValidationResourceType;
        ValidationResourceType = type;

        type ??= oldType;

        if (type != null)
        {
            var prop = type.GetProperty("Culture", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            prop?.SetValue(null, Culture);
        }
    }

    #endregion
}
