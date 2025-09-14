using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Results;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that validation rules should be applied to each element of a collection.
/// </summary>
/// <typeparam name="T">The type of the elements in the collection.</typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class CollectionValidatorAttribute<T> : CollectionValidatorBase<T>
{
    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not IEnumerable collection || collection is string || !CountHelper.TryGetCount(collection, out _))
            return this.GetFailedValidationResult(context, MessageResolver);

        var errors = ValidateCollection(collection, context);
        return GetValidationResult(errors);
    }

    private List<FluentValidationFailure> ValidateCollection(IEnumerable collection, ValidationContext parentContext)
    {
        parentContext.Items.TryGetValue(ItemPathKey, out var prefixValue);
        var pathPrefix = prefixValue is string s && s.Length > 0 ? $"{s}." : string.Empty;
        var parentMemberName = parentContext.MemberName;
        List<FluentValidationFailure> validationErrors = [];

        foreach (var rule in Rules)
        {
            var attr = rule.Validator;
            if (attr is null) continue;

            var member = rule.Member;
            var memberName = member.Name;
            var parentIndex = 0;

            foreach (var item in collection)
            {
                if (!rule.ShouldValidate(item)) { parentIndex++; continue; }

                string itemPath = BuildItemPath(pathPrefix, parentMemberName, parentIndex);
                var memberValue = member.GetValue(item);

                if (memberValue is IEnumerable innerCollection && innerCollection is not string)
                {
                    if (prefixValue is null)
                        parentContext.Items[ItemPathKey] = itemPath;

                    RecurseValidate(innerCollection, validationErrors,
                        rule, item, memberValue, attr, parentIndex, parentContext
                    );

                    parentIndex++;
                    continue;
                }

                var childContext = new ValidationContext(item) { MemberName = memberName };
                childContext.Items[ItemPathKey] = itemPath;

                var result = attr.GetValidationResult(memberValue, childContext);

                if (result != ValidationResult.Success)
                    validationErrors.Add(ResolveError(rule, item, memberValue, attr, result!.ErrorMessage, itemPath));

                parentIndex++;
            }
        }

        return validationErrors;
    }

    private void RecurseValidate(IEnumerable collection, List<FluentValidationFailure> errors,
    IValidationRule<T> currentRule, object parentItem, object? itemValue, ValidationAttribute attr,
    int parentIndex, ValidationContext parentContext)
    {
        var member = currentRule.Member;
        var memberName = member.Name;

        parentContext.Items.TryGetValue(ItemPathKey, out var itemPath);
        string pathPrefix = itemPath is string s && s.Length > 0 ? $"{s}." : $"{parentContext.MemberName}[{parentIndex}].";

        foreach (var innerRule in Rules.Where(r => member.AreSameMembers(r.Member)))
        {
            if (innerRule.Validator is null || !innerRule.ShouldValidate(collection)) continue;

            var context = new ValidationContext(collection) { MemberName = memberName };
            var segment = BuildItemPath(pathPrefix, memberName, -1); // -1 for collection-level path
            context.Items[ItemPathKey] = segment;

            var result = innerRule.Validator.GetValidationResult(collection, context);

            if (result != ValidationResult.Success)
                errors.Add(ResolveError(currentRule, parentItem, itemValue, attr, result?.ErrorMessage, segment));
        }

        var validator = CreateValidator(GetCollectionItemType(collection));
        int collectionIndex = 0;

        foreach (var item in collection)
        {
            var segment = BuildItemPath(pathPrefix, memberName, collectionIndex);
            var childContext = new ValidationContext(item);
            childContext.Items[ItemPathKey] = segment;

            var result = validator.Validate(childContext);

            if (!result.IsValid)
            {
                errors.AddRange(ProcessErrors(result.Errors, segment));
            }

            collectionIndex++;
        }
    }
}