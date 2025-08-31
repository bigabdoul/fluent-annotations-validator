using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Tests.Models;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Validators;

public class MockValidationBehaviorOptions(ValidationBehaviorOptions options)
{
    public ValidationBehaviorOptions Options => options;

    public List<(MemberInfo Member, ConditionalValidationRule Rule)> AddedRules => GetAddedRules<ValidationTypeConfiguratorTestModel>();

    public List<(MemberInfo Member, ConditionalValidationRule Rule)> GetAddedRules<T>()
    {
        List<(MemberInfo Member, ConditionalValidationRule Rule)>? added = [];
        var ruleTuples = options.EnumerateRules<T>();
        foreach (var (member, ruleList) in ruleTuples)
        {
            foreach (var r in ruleList)
            {
                added.Add((member, r));
            }
        }
        return added;
    }
}
