using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Results;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that validation rules should be applied asynchronously to each element of a collection.
/// </summary>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class CollectionValidatorAsyncAttribute<T> : CollectionValidatorBase<T>, IAsyncValidationAttribute
{
    /// <summary>
    /// Asynchronously validates the collection and its nested elements.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">The validation context.</param>
    /// <param name="cancellationToken">An object that propagates notification that operations should be canceled.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or failure.</returns>
    public async Task<ValidationResult?> ValidateAsync(object? value, ValidationContext context, CancellationToken cancellationToken)
    {
        if (value is null) return ValidationResult.Success;
        
        if (value is not IEnumerable collection || collection is string || !CountHelper.TryGetCount(collection, out _))
            return this.GetFailedValidationResult(context, MessageResolver);

        var errors = await ValidateCollectionAsync(collection, context, cancellationToken);
        if (errors.Count == 0) return ValidationResult.Success;

        Errors.AddRange(errors);
        return new ValidationResult($"Validation errors occurred: {errors.Count}. See Errors for details.");
    }

    private async Task<List<FluentValidationFailure>> ValidateCollectionAsync(
    IEnumerable collection, ValidationContext parentContext,
    CancellationToken cancellationToken)
    {
        List<FluentValidationFailure> validationErrors = [];

        string? parentMemberName = parentContext.MemberName;
        parentContext.Items.TryGetValue(ItemPathKey, out var prefixValue);
        string pathPrefix = prefixValue is string s && s.Length > 0 ? $"{s}." : string.Empty;

        foreach (var rule in Rules)
        {
            var attr = rule.Validator;
            if (attr is null) continue;

            var member = rule.Member;
            var memberName = member.Name;

            int parentIndex = -1;

            foreach (var item in collection)
            {
                parentIndex++;

                if (!await rule.ShouldValidateAsync(item, cancellationToken)) continue;

                string itemPath = BuildItemPath(pathPrefix, parentMemberName, parentIndex);
                var memberValue = member.GetValue(item);

                if (memberValue is IEnumerable inner && inner is not string)
                {
                    if (prefixValue is null)
                        parentContext.Items[ItemPathKey] = itemPath;

                    await RecurseValidateAsync(
                        inner, validationErrors, rule, item, memberValue, 
                        attr, parentIndex, parentContext, cancellationToken
                    );

                    continue;
                }

                var childContext = new ValidationContext(item) { MemberName = memberName };
                childContext.Items[ItemPathKey] = itemPath;

                var result = attr is IAsyncValidationAttribute asyncAttr
                    ? await asyncAttr.ValidateAsync(memberValue, childContext, cancellationToken)
                    : attr.GetValidationResult(memberValue, childContext);

                if (result != ValidationResult.Success)
                    validationErrors.Add(ResolveError(rule, item, memberValue, attr, result!.ErrorMessage, itemPath));
            }
        }

        return validationErrors;
    }

    private async Task RecurseValidateAsync(IEnumerable collection, List<FluentValidationFailure> errors,
        IValidationRule<T> currentRule, object parentItem, object? itemValue, ValidationAttribute attr,
        int parentIndex, ValidationContext parentContext, CancellationToken cancellationToken)
    {
        var member = currentRule.Member;
        var memberName = member.Name;

        parentContext.Items.TryGetValue(ItemPathKey, out var itemPath);

        string pathPrefix = itemPath is string s && s.Length > 0
            ? $"{s}." : $"{parentContext.MemberName}[{parentIndex}].";

        foreach (var innerRule in Rules.Where(r => member.AreSameMembers(r.Member)))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var innerAttr = innerRule.Validator;

            if (innerAttr is null || !await innerRule.ShouldValidateAsync(collection, cancellationToken)) continue;

            var context = new ValidationContext(collection) { MemberName = memberName };
            string segment = BuildItemPath(pathPrefix, memberName, -1); // -1 for collection-level path
            context.Items[ItemPathKey] = segment;

            var result = innerAttr is IAsyncValidationAttribute asyncAttr
                ? await asyncAttr.ValidateAsync(itemValue, context, cancellationToken)
                : innerAttr.GetValidationResult(collection, context);

            if (result != ValidationResult.Success)
                errors.Add(ResolveError(currentRule, parentItem, itemValue, attr, result?.ErrorMessage, segment));
        }

        var validator = CreateValidator(GetCollectionItemType(collection));
        int collectionIndex = 0;

        foreach (var item in collection)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string segment = BuildItemPath(pathPrefix, memberName, collectionIndex);
            var childContext = new ValidationContext(item);
            childContext.Items[ItemPathKey] = segment;

            var result = await validator.ValidateAsync(childContext, cancellationToken);

            if (!result.IsValid)
            {
                errors.AddRange(ProcessErrors(result.Errors, segment));
            }

            collectionIndex++;
        }
    }
}