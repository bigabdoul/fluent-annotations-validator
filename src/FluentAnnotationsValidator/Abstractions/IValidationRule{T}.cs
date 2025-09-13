namespace FluentAnnotationsValidator.Abstractions;

/// <summary>
/// Defines a strongly typed contract for validation rule metadata and evaluation logic.
/// </summary>
/// <typeparam name="T">The type of the instance being validated.</typeparam>
/// <remarks>
/// This interface extends <see cref="IValidationRule"/> with type-safe predicates and configuration.
/// It is used during rule construction and execution for fluent validation pipelines.
/// </remarks>
public interface IValidationRule<T> : IValidationRule
{
    /// <summary>
    /// Gets or sets a predicate that determines whether the rule should be applied to a given instance.
    /// </summary>
    new Predicate<T> Condition { get; set; }

    /// <summary>
    /// Gets or sets an asynchronous predicate that determines whether the rule should be applied.
    /// </summary>
    new Func<T, CancellationToken, Task<bool>>? AsyncCondition { get; set; }

    /// <summary>
    /// Sets a custom predicate used to determine rule applicability.
    /// </summary>
    /// <param name="predicate">A predicate that evaluates the target instance.</param>
    void SetShouldValidate(Predicate<T> predicate);

    /// <summary>
    /// Sets a custom asynchronous predicate used to determine rule applicability.
    /// </summary>
    /// <param name="predicate">An asynchronous predicate that evaluates the target instance.</param>
    void SetShouldAsyncValidate(Func<T, CancellationToken, Task<bool>> predicate);
}