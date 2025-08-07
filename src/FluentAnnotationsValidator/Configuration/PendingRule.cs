using FluentAnnotationsValidator.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Configuration;

internal sealed class PendingRule<T>(
    Expression member,
    Func<T, bool> predicate,
    string? message = null,
    string? key = null,
    string? resourceKey = null,
    Type? resourceType = null,
    CultureInfo? culture = null,
    string? fallbackMessage = null,
    bool? useConventionalKeys = true
)
{
    public Expression Member { get; set; } = member;
    public Func<T, bool> Predicate { get; set; } = predicate;
    public string? Message { get; set; } = message;
    public string? Key { get; set; } = key;
    public string? ResourceKey { get; set; } = resourceKey;
    public Type? ResourceType { get; set; } = resourceType;
    public CultureInfo? Culture { get; set; } = culture;
    public string? FallbackMessage { get; set; } = fallbackMessage;
    public bool? UseConventionalKeys { get; set; } = useConventionalKeys;

    /// <summary>
    /// Gets the list of dynamically added attributes via fluent rules.
    /// </summary>
    public List<ValidationAttribute> Attributes { get; } = [];

    public override int GetHashCode()
    {
        var member = Member.GetMemberInfo();
        return HashCode.Combine(member.Name, member.DeclaringType);
    }

    public override bool Equals(object? obj)
        => obj is PendingRule<T> other && Equals(other);

    public bool Equals(PendingRule<T>? other)
    {
        if (other == null) return false;

        var member1 = Member.GetMemberInfo();
        var member2 = other.Member.GetMemberInfo();

        return
            member1.Name == member2.Name &&
            member1.DeclaringType == member2.DeclaringType;
    }
}