using FluentAnnotationsValidator.Abstractions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Configuration
{
    public interface IValidationRuleGroupRegistry : IRuleRegistry
    {
        void AddRule(MemberInfo member, IValidationRule rule);
        void AddRule(Type objectType, MemberInfo member, IValidationRule rule);
        void AddRules(Type type, IList<IValidationRuleGroup> group);
        void Clear();
        bool Contains<T>(Expression<Func<T, string?>> expression, Func<IValidationRuleGroup, bool>? filter = null);
        bool ContainsAny<T>(Type type, MemberInfo member, Func<IValidationRule, bool> filter);
        List<(Type ModelType, ValidationRuleGroupList Groups)> EnumerateRules(Type type);
        List<(Type ModelType, ValidationRuleGroupList Rules)> EnumerateRules<T>();
        ValidationRuleGroupList FindRuleGroups(Expression expression, Func<IValidationRuleGroup, bool>? filter = null);
        ValidationRuleGroupList FindRules(Type objectType, MemberInfo member, Func<IValidationRuleGroup, bool>? filter = null);
        ValidationRuleGroupList GetRules(Type objectType);
        IReadOnlyList<IValidationRule> GetRules<T>(Expression<Func<T, string?>> expression, Func<IValidationRule, bool>? filter = null);
        ValidationRuleGroupList GetRules<T>(Expression<Func<T, string?>> expression, Func<IValidationRuleGroup, bool>? predicate = null);
        IEnumerable<IGrouping<MemberInfo, IValidationRule>> GetRulesByMember(Type forType);
        List<IValidationRule> GetRulesForType(Type type);
        List<IValidationRule<T>> GetRulesForType<T>();
        bool RemoveAll(Type type, MemberInfo member);
        int RemoveAll(Type objectType, MemberInfo member, Type attributeType);
        int RemoveAll(Type objectType, Predicate<MemberInfo> predicate);
        int RemoveAll<TAttribute>(Func<MemberInfo, TAttribute, bool> predicate) where TAttribute : ValidationAttribute;
        int RemoveAll<TAttribute>(Type objectType) where TAttribute : ValidationAttribute;
        int RemoveAllForType(Type objectType, Predicate<MemberInfo>? filter = null);
        ConcurrentDictionary<Type, IList<IValidationRuleGroup>> GetRegistryForMember(Type objectType, MemberInfo member);
        bool TryGetRules<T>(Expression<Func<T, string?>> expression, out IReadOnlyList<IValidationRule> rules, Func<IValidationRule, bool>? filter = null);
    }
}