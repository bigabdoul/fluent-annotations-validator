using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration;

/// <summary>
/// Represents a typed list of <see cref="IValidationRuleGroup"/> objects, providing utilities for rule management,
/// attribute filtering, and group merging.
/// </summary>
/// <remarks>
/// Each instance is keyed by a specific model <see cref="Type"/>, allowing targeted operations across validation rule groups.
/// This class supports fluent rule registration, override-safe merging, and predicate-based removal for granular control.
/// </remarks>
/// <param name="type">The model type used as the key for this rule group list.</param>
public class ValidationRuleGroupList(Type type) : IList<IValidationRuleGroup>
{
    private readonly List<IValidationRuleGroup> InternalList = [];

    /// <summary>
    /// Gets the model type associated with this rule group list.
    /// </summary>
    /// <example><c>typeof(UserProfileDto)</c></example>
    public Type Key { get; private set; } = type ?? throw new ArgumentNullException(nameof(type));

    /// <summary>
    /// Adds multiple <see cref="IValidationRuleGroup"/> instances to the end of the list.
    /// </summary>
    /// <param name="collection">The collection of rule groups to append.</param>
    public void AddRange(IEnumerable<IValidationRuleGroup> collection)
    {
        foreach (var group in collection)
        {
            if (group.ObjectType != Key)
                ThrowObjectTypeMismatch();
            InternalList.Add(group);
        }
    }

    /// <summary>
    /// Removes all rules of the specified attribute type from a given member.
    /// </summary>
    /// <typeparam name="TAttrribute">The type of <see cref="ValidationAttribute"/> to remove.</typeparam>
    /// <param name="member">The member whose rules should be removed.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAttributesOf<TAttrribute>(MemberInfo member) where TAttrribute : ValidationAttribute
        => RemoveAttributesOf(member, typeof(TAttrribute));

    /// <summary>
    /// Removes all rules of a specific attribute type that satisfy a given predicate from all members in the list.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the validation attribute to remove.</typeparam>
    /// <param name="predicate">A function that tests each rule's member and attribute for a condition.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAttributesOf<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate)
        where TAttribute : ValidationAttribute
    {
        var removedCount = 0;
        for (int i = InternalList.Count - 1; i >= 0; i--)
        {
            removedCount += InternalList[i].RemoveAttributesOf(predicate);
        }
        return removedCount;
    }

    /// <summary>
    /// Removes all rules of a specific attribute type from a given member.
    /// </summary>
    /// <param name="member">The member from which to remove rules.</param>
    /// <param name="attributeType">The type of the validation attribute to remove.</param>
    /// <returns>The number of rules removed.</returns>
    public int RemoveAttributesOf(MemberInfo member, Type attributeType)
    {
        var removedCount = 0;
        for (int i = InternalList.Count - 1; i >= 0; i--)
        {
            removedCount += InternalList[i].RemoveAttributesOf(member, attributeType);
        }
        return removedCount;
    }

    /// <summary>
    /// Removes all rules for a member that match a given predicate and belong to a specific model type.
    /// </summary>
    /// <param name="predicate">A function that evaluates each member for removal.</param>
    /// 
    /// <returns>The number of rules removed.</returns>
    public int RemoveRulesForMember(Predicate<MemberInfo> predicate)
    {
        var removed = 0;
        for (int i = InternalList.Count - 1; i >= 0; i--)
        {
            removed += InternalList[i].RemoveRules(predicate);
        }
        return removed;
    }

    /// <summary>
    /// Merges a single <see cref="IValidationRuleGroup"/> into the current list.
    /// </summary>
    /// <param name="group">The rule group to merge.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the group's <c>ObjectType</c> does not match the list's <see cref="Key"/>.
    /// </exception>
    public void Merge(IValidationRuleGroup group) => Merge([group]);

    /// <summary>
    /// Merges multiple <see cref="IValidationRuleGroup"/> instances into the current list.
    /// </summary>
    /// <param name="groups">The collection of rule groups to merge.</param>
    /// <remarks>
    /// Existing groups with matching <c>ObjectType</c> and member identity will have their rules combined.
    /// New groups are added if no match is found.
    /// </remarks>
    public void Merge(IEnumerable<IValidationRuleGroup> groups)
    {
        if (InternalList.Count == 0)
        {
            AddRange(groups);
        }
        else
        {
            // Build a quick lookup dictionary for the existing groups.
            var existingGroups = this.ToDictionary(
                g => (g.ObjectType, g.Member.AreSameMembersKey()),
                g => g);

            var objectType = Key;

            foreach (var incomingGroup in groups)
            {
                if (incomingGroup.ObjectType != objectType)
                    ThrowObjectTypeMismatch();

                var incomingKey = (objectType, incomingGroup.Member.AreSameMembersKey());
                if (existingGroups.TryGetValue(incomingKey, out var matchingGroup))
                {
                    // Match found, merge the rules.
                    matchingGroup.Rules.AddRange(incomingGroup.Rules);
                }
                else
                {
                    // No match, add the new group to the list.
                    InternalList.Add(incomingGroup);
                }
            }
        }
    }

    /// <summary>
    /// Removes all rule groups that satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">A function that evaluates each rule group for removal.</param>
    /// <returns>The number of rule groups removed.</returns>
    public int RemoveAll(Predicate<IValidationRuleGroup> predicate) 
        => InternalList.RemoveAll(predicate);

    #region IList<IValidationRuleGroup>

    /// <inheritdoc/>
    public int Count => InternalList.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public IValidationRuleGroup this[int index]
    {
        get => InternalList[index];
        set => InternalList[index] = value;
    }

    /// <inheritdoc/>
    public int IndexOf(IValidationRuleGroup item) => InternalList.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, IValidationRuleGroup item) => InternalList.Insert(index, item);

    /// <inheritdoc/>
    public void RemoveAt(int index) => InternalList.RemoveAt(index);

    /// <summary>
    /// Adds a validation rule group to the list by merging it with existing entries.
    /// </summary>
    /// <param name="item">The rule group to add.</param>
    public void Add(IValidationRuleGroup item) => Merge(item);

    /// <inheritdoc/>
    public void Clear() => InternalList.Clear();

    /// <inheritdoc/>
    public bool Contains(IValidationRuleGroup item) => InternalList.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(IValidationRuleGroup[] array, int arrayIndex) => InternalList.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public bool Remove(IValidationRuleGroup item) => InternalList.Remove(item);

    /// <inheritdoc/>
    public IEnumerator<IValidationRuleGroup> GetEnumerator() => InternalList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    [DoesNotReturn]
    private void ThrowObjectTypeMismatch()
    {
        throw new InvalidOperationException($"Object Type Mismatch: All types in the collection must be of the same type {Key.Name}.");
    }
}
