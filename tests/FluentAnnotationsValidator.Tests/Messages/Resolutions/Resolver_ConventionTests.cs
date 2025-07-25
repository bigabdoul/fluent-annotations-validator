using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Messages;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;

public class Resolver_ConventionTests
{
    const string EmailName = nameof(LoginDtoWithResource.Email);

    [Fact]
    public void Resolves_Using_Convention_When_Enabled()
    {
        var resolver = new ValidationMessageResolver(new ValidationBehaviorOptions());
        var msg = resolver.ResolveMessage(typeof(LoginDtoWithResource), EmailName, new RequiredAttribute());
        Assert.Equal(ConventionValidationMessages.Email_Required, msg);
    }

    [Fact]
    public void Skips_Convention_If_Disabled()
    {
        var rule = new ConditionalValidationRule(
            dto => true,
            UseConventionalKeyFallback: false
        );

        var resolver = new ValidationMessageResolver(new ValidationBehaviorOptions());
        var msg = resolver.ResolveMessage(typeof(LoginDtoWithResource), EmailName, new RequiredAttribute(), rule);

        Assert.Equal($"Invalid value for {EmailName}", msg);
    }
}
