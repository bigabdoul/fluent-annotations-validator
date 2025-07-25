using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;
using static TestHelpers;

public class Resolver_PriorityTests
{
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

        // Act
        var msg = GetMessageResolver().ResolveMessage<TestLoginDto>(x => x.Email, new RequiredAttribute(), rule);

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

        var attr = new RequiredAttribute
        {
            ErrorMessageResourceName = nameof(WrongMessages.WrongKey), 
            ErrorMessageResourceType = typeof(WrongMessages)
        };

        // Act
        var msg = GetMessageResolver().ResolveMessage<TestLoginDto>(x => x.Email, attr, rule);

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

        // Act
        var msg = GetMessageResolver().ResolveMessage<TestLoginDto>(x => x.Email, attr);

        Assert.Equal(ValidationMessages.EmailRequired, msg);
    }

    [Fact]
    public void Convention_Fallback_Used_WhenNoResourceSpecified()
    {
        var resolver = GetMessageResolver();

        // Act
        var msg = resolver.ResolveMessage<TestLoginDtoWithResource>(x => x.Email, new RequiredAttribute());

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

        var resolver = GetMessageResolver();

        // Act
        var msg = resolver.ResolveMessage<TestLoginDto>(x => x.Email, new RequiredAttribute(), rule);

        Assert.Equal("Use this instead", msg);
    }
}
