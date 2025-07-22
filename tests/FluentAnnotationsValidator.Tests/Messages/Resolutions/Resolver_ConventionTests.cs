using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;

public class Resolver_ConventionTests
{
    [Fact]
    public void Resolves_Using_Convention_When_Enabled()
    {
        var attr = new RequiredAttribute();
        var info = new PropertyValidationInfo
        {
            Property = typeof(LoginDtoWithResource).GetProperty(nameof(LoginDtoWithResource.Email))!,
            TargetModelType = typeof(LoginDtoWithResource)
        };

        var resolver = new ValidationMessageResolver();
        var msg = resolver.ResolveMessage(info, attr);
        Assert.Equal("Email is required (convention).", msg);
    }

    [Fact]
    public void Skips_Convention_If_Disabled()
    {
        var attr = new RequiredAttribute();
        var rule = new ConditionalValidationRule(
            dto => true,
            UseConventionalKeyFallback: false
        );

        var info = new PropertyValidationInfo
        {
            Property = typeof(LoginDtoWithResource).GetProperty(nameof(LoginDtoWithResource.Email))!,
            TargetModelType = typeof(LoginDtoWithResource)
        };

        var resolver = new ValidationMessageResolver();
        var msg = resolver.ResolveMessage(info, attr, rule);

        Assert.Equal("Invalid value for Email", msg);
    }
}
