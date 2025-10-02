using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FluentAnnotationsValidator.Runtime.Annotations;

using FluentAnnotationsValidator.Annotations;
using Core.Extensions;
using Core.Interfaces;
using Core.Results;
using Runtime;

/// <summary>
/// Base class for collection validators that apply rules to each element.
/// </summary>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
public abstract partial class CollectionValidationAttribute<T> : FluentValidationAttribute
{
    private IServiceProvider? _serviceProvider;
    
    /// <summary>
    /// Gets the items path dictionary key.
    /// </summary>
    protected const string ItemPathKey = "ItemPath";

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public virtual IServiceProvider ServiceProvider => 
        _serviceProvider ??= FluentAnnotationsBuilder.Default?.Services.BuildServiceProvider()
            ?? throw new InvalidOperationException("No service provider available.");

    /// <summary>
    /// Gets the collection of rules that apply to each element.
    /// </summary>
    public List<IValidationRule<T>> Rules { get; } = [];

    /// <summary>
    /// This method determines the type of the elements in a collection.
    /// </summary>
    /// <param name="collection">The collection of objects to inspect.</param>
    /// <returns>The type of the collection's elements.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the collection's element type cannot be determined.
    /// </exception>
    protected Type GetCollectionItemType(System.Collections.IEnumerable collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var type = collection.GetType();

        // If it's a generic collection, all items are guaranteed to be of the same type.
        if (type.IsGenericType) return type.GetGenericArguments()[0];

        // Return the first non-null item's type.
        // This operation does not guarantee that all items are of the same type.
        // To enforce that constraint, we need a parameter to enable checking for
        // consistency and avoid hard-to-debug exceptions later down the road.
        foreach (var item in collection)
            if (item is not null) return item.GetType();

        // All items in the collection are null. Should we return null, indicating
        // that all items in the collection are useless? Maybe there's rule for that, too?
        // If we can't determine the type of the items in the collection, we won't get
        // any validator for that type anyway; so throwing is the more obvious thing to do.
        throw new InvalidOperationException("Unable to determine the type of the collection's elements.");
    }

    /// <summary>
    /// Resolves a localized error message for a validation rule failure.
    /// </summary>
    /// <param name="rule">The validation rule that failed.</param>
    /// <param name="item">The item instance being validated.</param>
    /// <param name="attr">The validation attribute that was applied to the member.</param>
    /// <param name="fallbackMessage">The default message to use if a localized message cannot be resolved.</param>
    /// 
    /// <returns>A formatted error message for the validation failure.</returns>
    protected string ResolveMessage(IValidationRule<T> rule, object item, ValidationAttribute attr, string? fallbackMessage)
    {
        return 
            //(index < 0 ? "" : $"[{index}]: ") +
            (MessageResolver != null
                ? MessageResolver.ResolveMessage(item, rule.GetPropertyName(), attr, rule)
                : fallbackMessage ?? string.Empty);
    }

    /// <summary>
    /// Creates a structured <see cref="FluentValidationFailure"/>, enriched with contextual metadata.
    /// </summary>
    /// <param name="rule">The validation rule that triggered the failure.</param>
    /// <param name="itemInstance">The object instance being validated.</param>
    /// <param name="attemptedValue">The value that failed validation.</param>
    /// <param name="attr">The <see cref="ValidationAttribute"/> responsible for the failure.</param>
    /// <param name="fallbackMessage">A fallback error message if none is resolved from the attribute.</param>
    /// <param name="segment">The path segment representing the item's location in the object graph.</param>
    /// 
    /// <returns>An initialized instance of <see cref="FluentValidationFailure"/>.</returns>
    /// <remarks>
    /// This method constructs a <see cref="FluentValidationFailure"/> with diagnostic metadata including origin,
    /// collection index, and a fully qualified <c>ItemPathKey</c>. It ensures consistent error formatting for nested validation scenarios.
    /// </remarks>
    protected FluentValidationFailure ResolveError(IValidationRule<T> rule, object itemInstance,
    object? attemptedValue, ValidationAttribute attr, string? fallbackMessage, string? segment)
    {
        var (collectionIndex, parentIndex) = ExtractIndices(segment);
        if (!string.IsNullOrEmpty(segment)) segment += ".";

        var failure = new FluentValidationFailure(rule.Member.Name, ResolveMessage(rule, itemInstance, attr, fallbackMessage), attemptedValue)
        {
            CustomState = new Dictionary<string, object>
            {
                { "Origin", attr is null ? string.Empty : attr.CleanAttributeName() },
                { ItemPathKey, $"{segment}{rule.Member.Name}" }
            },
            CollectionIndex = collectionIndex,
            ParentCollectionIndex = parentIndex,
            AttemptedValue = attemptedValue,
        };

        return failure;
    }

    /// <summary>
    /// Processes validation errors from a <see cref="FluentValidationResult"/> and enriches them with contextual metadata.
    /// </summary>
    /// <param name="errors">The result containing validation errors to process.</param>
    /// <param name="segment">The path segment representing the current item's location in the object graph.</param>
    /// <remarks>
    /// This method ensures that each error includes accurate <c><see cref="FluentValidationFailure.CollectionIndex"/></c>, 
    /// <c><see cref="FluentValidationFailure.ParentCollectionIndex"/></c>, and a fully qualified <c>ItemPath</c> key in 
    /// <see cref="FluentValidationFailure.CustomState"/> for diagnostics and UI mapping.
    /// </remarks>
    protected IEnumerable<FluentValidationFailure> ProcessErrors(IEnumerable<FluentValidationFailure> errors, string segment)
    {
        var (collectionIndex, parentIndex) = ExtractIndices(segment);

        foreach (var err in errors)
        {
            err.CollectionIndex = err.CollectionIndex == null ? collectionIndex : err.CollectionIndex;
            err.ParentCollectionIndex = err.ParentCollectionIndex == null ? parentIndex : err.ParentCollectionIndex;

            err.CustomState ??= [];
            err.CustomState.TryAdd(ItemPathKey, $"{segment}.{err.PropertyName}");
            yield return err;
        }
    }

    /// <summary>
    /// Constructs a fully qualified item path for diagnostics and validation context tracking.
    /// </summary>
    /// <param name="prefix">An optional prefix representing the parent path segment (e.g., from a containing collection).</param>
    /// <param name="memberName">The name of the member being validated.</param>
    /// <param name="index">
    /// The index of the item within the collection. If negative, the index is omitted and only the member path is returned.
    /// </param>
    /// <returns>
    /// A formatted string representing the item's path in the object graph, suitable for use in <c>ItemPathKey</c> or diagnostics.
    /// </returns>
    /// <remarks>
    /// This method ensures consistent path formatting across nested validation layers, supporting both indexed and non-indexed segments.
    /// </remarks>
    protected static string BuildItemPath(string prefix, string? memberName, int index)
    {
        return index < 0
            ? $"{prefix}{memberName}"
            : string.IsNullOrEmpty(prefix)
                ? $"{memberName}[{index}]"
                : $"{prefix}{memberName}[{index}]";
    }

    /// <summary>
    /// Instantiates a type-specific <see cref="IFluentValidator"/> using the configured <see cref="ServiceProvider"/>.
    /// </summary>
    /// <param name="objectType">The target type for which a validator should be created.</param>
    /// <returns>
    /// A configured <see cref="IFluentValidator"/> instance capable of validating objects of the specified type.
    /// </returns>
    protected IFluentValidator CreateValidator(Type objectType)
    {
        var validatorType = typeof(IFluentValidator<>).MakeGenericType(objectType);
        var validator = (IFluentValidator)ServiceProvider.GetRequiredService(validatorType);

        if (RuleRegistry != null)
            validator.SetRuleRegistry(RuleRegistry);

        if (MessageResolver != null)
            validator.SetMessageResolver(MessageResolver!);

        return validator;
    }

    private static (int? collectionIndex, int? parentIndex) ExtractIndices(string? segment)
    {
        if (segment == null) return (null, null);

        int? collectionIndex = null, parentIndex = null;
        var indices = GetIndicesFromSegment(segment);

        if (indices.Length > 0)
        {
            collectionIndex = indices[^1];
            if (indices.Length > 1)
                parentIndex = indices[^2];
        }

        return (collectionIndex, parentIndex);
    }

    internal static int[] GetIndicesFromSegment(string segment)
    {
        // segment = "Items[4].Products[0].Orders[2].Quantity";
        var matches = SegmentIndicesRegex().Matches(segment);
        return [.. matches.Select(m => m.Groups[1].Value).Select(int.Parse)];
    }

    [GeneratedRegex(@"\[(\d+)\]")]
    private static partial Regex SegmentIndicesRegex();
}