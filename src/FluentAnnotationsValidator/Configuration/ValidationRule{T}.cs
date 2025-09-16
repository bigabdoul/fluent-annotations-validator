using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using System.Globalization;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a strongly-typed validation rule that applies to a specific member of a DTO.
/// </summary>
/// <typeparam name="T">The type of the target instance being validated.</typeparam>
/// <remarks>
/// This class encapsulates metadata, conditions, and message resolution logic for a single validation rule.
/// It supports both synchronous and asynchronous evaluation, localized error messages, and override-safe diagnostics.
/// </remarks>
/// <param name="condition">A predicate that determines whether the rule should be applied to a given instance.</param>
/// <param name="expression">A lambda expression identifying the member being validated (e.g., <c>x => x.Property</c>).</param>
/// <param name="message">The explicit error message to display when validation fails.</param>
/// <param name="key">An optional failure key used for diagnostics or message resolution.</param>
/// <param name="resourceKey">The key used to retrieve a localized message from the resource manager.</param>
/// <param name="resourceType">The type containing localized resources (e.g., a <c>.resx</c>-backed class).</param>
/// <param name="culture">The culture to use when resolving localized messages.</param>
/// <param name="fallbackMessage">A fallback message used if localization or key resolution fails.</param>
/// <param name="useConventionalKeys">
/// Indicates whether conventional key lookup (e.g., <c>Property_Attribute</c>) should be used.
/// Set to <c>false</c> to disable fallback resolution.
/// </param>
public class ValidationRule<T>(
    Predicate<T> condition,
    Expression? expression,
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool? useConventionalKeys = true
    ) : ValidationRule(expression, condition: null, message, key, resourceKey, resourceType, culture, fallbackMessage, useConventionalKeys), IValidationRule<T>
{
    private Predicate<T>? _shouldApplyCondition;
    private Func<T, CancellationToken, Task<bool>>? _shouldApplyAsyncCondition;

    /// <inheritdoc />
    public new Predicate<T> Condition { get; set; } = condition ?? (_ => true);

    /// <inheritdoc />
    public new Func<T, CancellationToken, Task<bool>>? AsyncCondition { get; set; }

    /// <summary>
    /// Determines whether the current rule should be evaluated.
    /// </summary>
    /// <param name="targetInstance">The target instance passed to the predicate.</param>
    /// <returns>
    /// <see langword="true"/> if the rule should be evaluated; otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool ShouldApplyCondition(T targetInstance) =>
        (_shouldApplyCondition ?? Condition).Invoke(targetInstance);

    /// <inheritdoc/>
    public override bool ShouldValidate(object targetInstance)
    {
        ArgumentNullException.ThrowIfNull(targetInstance);
        return targetInstance is not T target || ShouldApplyCondition(target);// throw InvalidTargetInstance(targetInstance);
    }

    /// <summary>
    /// Asynchronously determines whether the current rule should be evaluated.
    /// </summary>
    /// <param name="targetInstance">The target instance passed to the predicate.</param>
    /// <param name="cancellationToken">An object that propagates notification that operations should be canceled.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to <see langword="true"/> if the rule should be evaluated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual Task<bool> ShouldApplyConditionAsync(T targetInstance, CancellationToken cancellationToken = default)
    {
        var currentAsyncCondition = AsyncCondition;

        if (_shouldApplyAsyncCondition is null && currentAsyncCondition is null)
            return Task.Run(() => ShouldApplyCondition(targetInstance!));

        return _shouldApplyAsyncCondition is not null
            ? _shouldApplyAsyncCondition(targetInstance, cancellationToken)
            : currentAsyncCondition!(targetInstance, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<bool> ShouldValidateAsync(object targetInstance, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(targetInstance);
        return targetInstance is not T target || await ShouldApplyConditionAsync(target, cancellationToken);
    }

    /// <summary>
    /// Overrides the default condition used to determine whether the rule should be applied.
    /// </summary>
    /// <param name="condition">A predicate that evaluates the target instance.</param>
    /// <remarks>
    /// This method is useful for injecting dynamic or context-specific logic during runtime or testing.
    /// </remarks>
    public void SetShouldValidate(Predicate<T> condition) => _shouldApplyCondition = condition;

    /// <summary>
    /// Sets the asynchronous function used to evaluate rule application.
    /// </summary>
    /// <param name="condition">
    /// An asynchronous function used to evaluate whether the current rule should be applied.
    /// </param>
    public void SetShouldAsyncValidate(Func<T, CancellationToken, Task<bool>> condition)
        => _shouldApplyAsyncCondition = condition;

    /// <summary>
    /// Returns a string representation of the rule, including its validator type and target member.
    /// </summary>
    /// <returns>
    /// A string in the format <c>[Attribute]ObjectType.Member</c> (e.g., <c>[Required]LoginModel.Email</c>), useful for debugging and diagnostics.
    /// </returns>
    public override string? ToString() =>
        (Validator != null ? $"[{Validator.CleanAttributeName()}]" : string.Empty) +
        $"{Member.ReflectedType?.Name}.{Member.Name}";
}