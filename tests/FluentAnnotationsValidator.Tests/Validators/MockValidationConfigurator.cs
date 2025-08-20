using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Tests.Models;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Validators;

public class MockValidationConfigurator(ValidationBehaviorOptions options) : ValidationConfigurator(options)
{
    public List<Action<ValidationBehaviorOptions>> RegisteredActions { get; } = [];

    public override void Register(Action<ValidationBehaviorOptions> action)
    {
        RegisteredActions.Add(action);
    }
}

public class MockValidationBehaviorOptions(ValidationBehaviorOptions options)
{
    public ValidationBehaviorOptions Options => options;

    public List<(MemberInfo Member, ConditionalValidationRule Rule)> AddedRules
    {
        get
        {
            List<(MemberInfo Member, ConditionalValidationRule Rule)>? added = [];
            var ruleTuples = options.EnumerateRules<ValidationTypeConfiguratorTestModel>();
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
}
