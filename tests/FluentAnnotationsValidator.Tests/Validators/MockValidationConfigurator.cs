using FluentAnnotationsValidator.Tests.Models;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Validators;

public class MockValidationRuleGroupRegistry(ValidationRuleGroupRegistry options)
{
    public ValidationRuleGroupRegistry Options => options;

    public List<(MemberInfo Member, IValidationRule Rule)> AddedRules => GetAddedRules<ValidationTypeConfiguratorTestModel>();

    public List<(MemberInfo Member, IValidationRule Rule)> GetAddedRules<T>()
    {
        List<(MemberInfo Member, IValidationRule Rule)>? added = [];
        var ruleTuples = options.EnumerateRules<T>();
        foreach (var (type, ruleList) in ruleTuples)
        {
            foreach (var group in ruleList)
            {
                foreach (var rule in group.Rules)
                {
                    added.Add((rule.Member, rule));
                }
            }
        }
        return added;
    }
}
