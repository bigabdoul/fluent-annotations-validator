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
public class ValidationTypeConfigurator<T>(ValidationConfigurator parent) : IValidationTypeConfigurator<T>
{
    private readonly List<PendingRule> _pendingRules = [];

    private PendingRule? _currentRule;
    private Type? _resourceType;
    private CultureInfo? _culture;
    private bool _useConventionalKeys = true;
    private string? _fallbackMessage;

    private record PendingRule(
        LambdaExpression Property,
        Func<T, bool> Predicate,
        string? Message = null,
        string? Key = null,
        string? ResourceKey = null,
        Type? ResourceType = null,
        CultureInfo? Culture = null,
        string? FallbackMessage = null,
        bool UseConventionalKeys = true
    );

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithValidationResource{TResource}()"/>
    public ValidationTypeConfigurator<T> WithValidationResource<TResource>()
    {
        _resourceType = typeof(TResource);
        AssignCultureTo(_resourceType);
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithValidationResource(Type?)"/>
    public ValidationTypeConfigurator<T> WithValidationResource(Type? resourceType)
    {
        _resourceType = resourceType;
        if (_resourceType != null)
            AssignCultureTo(_resourceType);
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithCulture(CultureInfo)"/>
    public ValidationTypeConfigurator<T> WithCulture(CultureInfo culture)
    {
        _culture = culture;
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.When{TProp}(Expression{Func{T, TProp}}, Func{T, bool})"/>
    public ValidationTypeConfigurator<T> When<TProp>(Expression<Func<T, TProp>> property, Func<T, bool> condition)
    {
        CommitCurrentRule();
        _currentRule = new PendingRule(
            Property: CastToObjectExpression(property),
            Predicate: model => condition(model),
            ResourceType: _resourceType,
            Culture: _culture,
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
            _currentRule = _currentRule with
            {
                Message = message,
                ResourceType = _resourceType,
                Culture = _culture,
                FallbackMessage = _fallbackMessage,
                UseConventionalKeys = _useConventionalKeys,
            };
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.WithKey(string)"/>
    public ValidationTypeConfigurator<T> WithKey(string key)
    {
        if (_currentRule is not null)
            _currentRule = _currentRule with
            {
                Key = key,
                ResourceType = _resourceType,
                Culture = _culture,
                FallbackMessage = _fallbackMessage,
                UseConventionalKeys = _useConventionalKeys,
            };
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Localized(string)"/>
    public ValidationTypeConfigurator<T> Localized(string resourceKey)
    {
        if (_currentRule is not null)
            _currentRule = _currentRule with
            {
                ResourceKey = resourceKey,
                ResourceType = _resourceType,
                Culture = _culture,
                FallbackMessage = _fallbackMessage,
                UseConventionalKeys = _useConventionalKeys,
            };
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
        return this;
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.For{TNext}"/>
    public ValidationTypeConfigurator<TNext> For<TNext>()
    {
        CommitCurrentRule();
        return parent.For<TNext>();
    }

    /// <inheritdoc cref="IValidationTypeConfigurator{T}.Build"/>
    /// <remarks>
    /// The list is populated only when the <see cref="ValidationBehaviorOptions"/>
    /// options is resolved, usually when an instance of <see cref="IServiceProvider"/>
    /// calls provider.GetService(typeof(IOptions&lt;ValidationBehaviorOptions&gt;));
    /// </remarks>
    public void Build()
    {
        CommitCurrentRule();

        foreach (var rule in _pendingRules)
        {
            // registration action if deferred until ValidationBehaviorOptions is resolved
            parent.Register(opts =>
            {
                opts.AddCondition(
                    rule.Property,
                    rule.Predicate,
                    rule.Message,
                    rule.Key,
                    rule.ResourceKey,
                    rule.ResourceType,
                    rule.Culture,
                    rule.FallbackMessage,
                    rule.UseConventionalKeys
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

    private void AssignCultureTo(Type type)
    {
        if (_culture is null) return;

        var prop = type.GetProperty("Culture", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        prop?.SetValue(null, _culture);
    }

    private static Expression<Func<T, object>> CastToObjectExpression<TProp>(Expression<Func<T, TProp>> expr)
    {
        return Expression.Lambda<Func<T, object>>(Expression.Convert(expr.Body, typeof(object)), expr.Parameters);
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
}
