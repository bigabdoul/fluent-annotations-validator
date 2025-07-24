using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;

public class Resolver_ConventionTests
{
    const string EmailName = nameof(LoginDtoWithResource.Email);

    [Fact]
    public void Resolves_Using_Convention_When_Enabled()
    {
        var attr = new RequiredAttribute();
        var info = new MemberValidationInfo
        {
            Member = typeof(LoginDtoWithResource).GetProperty(EmailName)!,
            DeclaringType = typeof(LoginDtoWithResource)
        };

        var resolver = new ValidationMessageResolver(new ValidationBehaviorOptions());
        var msg = resolver.ResolveMessage(info.DeclaringType, EmailName, attr);
        Assert.Equal(ConventionValidationMessages.Email_Required, msg);
    }

    [Fact]
    public void Skips_Convention_If_Disabled()
    {
        var attr = new RequiredAttribute();
        var rule = new ConditionalValidationRule(
            dto => true,
            UseConventionalKeyFallback: false
        );

        var info = new MemberValidationInfo
        {
            Member = typeof(LoginDtoWithResource).GetProperty(EmailName)!,
            DeclaringType = typeof(LoginDtoWithResource)
        };

        var resolver = new ValidationMessageResolver(new ValidationBehaviorOptions());
        var msg = resolver.ResolveMessage(info.DeclaringType, EmailName, attr, rule);

        Assert.Equal($"Invalid value for {EmailName}", msg);
    }
}
