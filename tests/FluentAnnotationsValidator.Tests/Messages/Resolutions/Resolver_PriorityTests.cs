using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;

public class Resolver_PriorityTests
{
    private static ValidationMessageResolver GetResolver() => new(new ValidationBehaviorOptions());

    private static class WrongMessages
    {
        public static string WrongKey => "Wrong!";
    }

    [Fact]
    public void Rule_Message_OverridesEverything()
    {
        var rule = new ConditionalValidationRule(
            dto => true,
            Message: "Override wins"
        );

        var attr = new RequiredAttribute();
        var info = CreateInfo<TestLoginDto>(x => x.Email);

        var msg = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr, rule);
        Assert.Equal("Override wins", msg);
    }

    [Fact]
    public void Rule_ResourceKey_OverridesAttributeResource()
    {
        var rule = new ConditionalValidationRule(
            dto => true,
            ResourceKey: nameof(ValidationMessages.EmailRequired),
            ResourceType: typeof(ValidationMessages)
        );

        var attr = new RequiredAttribute { ErrorMessageResourceName = nameof(WrongMessages.WrongKey), ErrorMessageResourceType = typeof(WrongMessages) };
        var info = CreateInfo<TestLoginDto>(x => x.Email);

        var msg = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr, rule);
        Assert.Equal(ValidationMessages.EmailRequired, msg);
    }

    [Fact]
    public void Attribute_Resource_OverridesConvention()
    {
        var attr = new RequiredAttribute
        {
            ErrorMessageResourceName = nameof(ValidationMessages.EmailRequired),
            ErrorMessageResourceType = typeof(ValidationMessages)
        };

        var info = CreateInfo<TestLoginDtoWithResource>(x => x.Email);
        var msg = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr);
        Assert.Equal(ValidationMessages.EmailRequired, msg);
    }

    [Fact]
    public void Convention_Fallback_Used_WhenNoResourceSpecified()
    {
        var attr = new RequiredAttribute();
        var info = CreateInfo<TestLoginDtoWithResource>(x => x.Email);

        var msg = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr);

        Assert.Equal(ConventionValidationMessages.Email_Required, msg); // uses conventional key
    }

    [Fact]
    public void FallbackMessage_Used_WhenResourceFails()
    {
        var rule = new ConditionalValidationRule(
            dto => true,
            ResourceKey: "MissingKey",
            ResourceType: typeof(WrongMessages),
            FallbackMessage: "Use this instead"
        );

        var attr = new RequiredAttribute();
        var info = CreateInfo<TestLoginDto>(x => x.Email);

        var msg = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr, rule);
        Assert.Equal("Use this instead", msg);
    }

    private static (MemberInfo Member, Type InstanceType) CreateInfo<T>(Expression<Func<T, string?>> expr) =>
        (expr.GetMemberInfo(), typeof(T));
}
