using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
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
    private readonly List<PendingRule> _pendingRules = [];
    private PendingRule? _currentRule;
    private bool _useConventionalKeys = true;
    private string? _fallbackMessage;

    private record PendingRule(
        Expression Member,
        Func<T, bool> Predicate,
        string? Message = null,
        string? Key = null,
        string? ResourceKey = null,
        Type? ResourceType = null,
        CultureInfo? Culture = null,
        string? FallbackMessage = null,
        bool? UseConventionalKeys = true
    );

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
        options.CommonCulture = Culture = culture;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.When{TProp}(Expression{Func{T, TProp}}, Func{T, bool})"/>
    public ValidationTypeConfigurator<T> When<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
    {
        CommitCurrentRule();
        _currentRule = new PendingRule(
            Member: property,
            Predicate: model => condition(model),
            ResourceType: ValidationResourceType,
            Culture: Culture,
            FallbackMessage: _fallbackMessage,
            UseConventionalKeys: _useConventionalKeys
        );
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.And{TProp}(Expression{Func{T, TProp}}, Func{T, bool})"/>
    public ValidationTypeConfigurator<T> And<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => When(property, condition);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Except{TProp}(Expression{Func{T, TProp}})"/>
    public ValidationTypeConfigurator<T> Except<TProp>(Expression<Func<T, TProp>> property)
        => When(property, _ => false);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.AlwaysValidate{TProp}(Expression{Func{T, TProp}})"/>
    public ValidationTypeConfigurator<T> AlwaysValidate<TProp>(Expression<Func<T, TProp>> property)
        => When(property, _ => true);

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithMessage(string)"/>
    public ValidationTypeConfigurator<T> WithMessage(string message)
    {
        if (_currentRule is not null)
            _currentRule = _currentRule with { Message = message };
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithKey(string)"/>
    public ValidationTypeConfigurator<T> WithKey(string key)
    {
        if (_currentRule is not null)
            _currentRule = _currentRule with { Key = key };
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Localized(string)"/>
    public ValidationTypeConfigurator<T> Localized(string resourceKey)
    {
        if (_currentRule is not null)
            _currentRule = _currentRule with { ResourceKey = resourceKey };
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.DisableConventionalKeys"/>
    public ValidationTypeConfigurator<T> DisableConventionalKeys()
    {
        options.UseConventionalKeys = _useConventionalKeys = false;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.UseFallbackMessage(string)"/>
    public ValidationTypeConfigurator<T> UseFallbackMessage(string fallbackMessage)
    {
        _fallbackMessage = fallbackMessage;
        if (_currentRule is not null)
            _currentRule = _currentRule with { FallbackMessage = _fallbackMessage };
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.For{TNext}"/>
    public ValidationTypeConfigurator<TNext> For<TNext>()
    {
        CommitCurrentRule();
        ValidationConfiguratorStore.Registry.Register(typeof(T), this);
        return parent.For<TNext>();
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Build"/>
    public void Build()
    {
        CommitCurrentRule();

        foreach (var rule in _pendingRules)
        {
            parent.Register(opts =>
            {
                AddRule(opts,
                    rule.Member,
                    rule.Predicate,
                    rule.Message,
                    rule.Key,
                    rule.ResourceKey,
                    rule.FallbackMessage,
                    rule.ResourceType ?? opts.CommonResourceType,
                    rule.Culture ?? opts.CommonCulture,
                    rule.UseConventionalKeys ?? opts.UseConventionalKeys
                );
            });
        }

        _pendingRules.Clear();
        parent.Build(); // finalize
    }

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

        options.CommonResourceType = ValidationResourceType;
        options.CommonCulture = Culture;
    }

    #region IValidationTypeConfigurator<T>

    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.When<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => When(property, condition);
    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.And<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
        => And(property, condition);
    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.Except<TProp>(Expression<Func<T, TProp>> property)
        => Except(property);
    IValidationTypeConfigurator<T> IValidationTypeConfigurator<T>.AlwaysValidate<TProp>(Expression<Func<T, TProp>> property)
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

    #region helper

    static void AddRule<TModel>(ValidationBehaviorOptions options,
        Expression memberExpression,
        Func<TModel, bool> predicate,
        string? message = null,
        string? key = null,
        string? resourceKey = null,
        string? fallbackMessage = null,
        Type? resourceType = null,
        CultureInfo? culture = null,
        bool useConventionalKeys = true)
    {
        var member = memberExpression.GetMemberInfo();

        var rule = new ConditionalValidationRule(model => predicate((TModel)model),
            message,
            key,
            resourceKey,
            resourceType,
            culture,
            fallbackMessage,
            useConventionalKeys)
        {
            Member = member,
        };

        options.AddRule(member, rule);
    }

    #endregion
}
