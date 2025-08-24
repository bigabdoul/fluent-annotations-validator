using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages;

public class ValidationMessageResolverTests
{
    private static ValidationMessageResolver GetResolver() => new(new ValidationBehaviorOptions());

    [Fact]
    public void ResolveMessage_Inline_ReturnsFormattedMessage()
    {
        // Arrange
        var attr = new RequiredAttribute { ErrorMessage = "Field {0} is required" };
        var info = new MemberValidationInfo
        {
            Member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            InstanceType = typeof(TestLoginDto)
        };

        // Act
        var message = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr);

        // Assert
        Assert.Equal("Field Email is required", message);
    }

    [Fact]
    public void ResolveMessage_AttributeResource_ReturnsFromResourceType()
    {
        var attr = new RequiredAttribute
        {
            ErrorMessageResourceName = nameof(ValidationMessages.EmailRequired),
            ErrorMessageResourceType = typeof(ValidationMessages)
        };

        var info = new MemberValidationInfo
        {
            Member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            InstanceType = typeof(TestLoginDto)
        };

        var message = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr);
        Assert.Equal(ValidationMessages.EmailRequired, message);
    }

    [Fact]
    public void ResolveMessage_ConditionalRuleMetadata_WinsOverAttribute()
    {
        var attr = new RequiredAttribute();
        var info = new MemberValidationInfo
        {
            Member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            InstanceType = typeof(TestLoginDto)
        };

        var rule = new ConditionalValidationRule(Predicate: null!, Message: "Overridden message");

        var message = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr, rule);
        Assert.Equal("Overridden message", message);
    }

    [Fact]
    public void ResolveMessage_ResourceKey_FromRuleType_FormatsCorrectly()
    {
        var attr = new StringLengthAttribute(5);
        var info = new MemberValidationInfo
        {
            Member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!,
            InstanceType = typeof(TestLoginDto)
        };

        var rule = new ConditionalValidationRule(Predicate: null!, 
            ResourceKey: nameof(ValidationMessages.PasswordRequired), 
            ResourceType: typeof(ValidationMessages));

        var message = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr, rule);
        Assert.Equal(ValidationMessages.PasswordRequired, message);
    }

    [Fact]
    public void ResolveMessage_Fallback_UsesConventionBasedResolution()
    {
        var attr = new RequiredAttribute();
        var info = new MemberValidationInfo
        {
            Member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            InstanceType = typeof(TestLoginDtoWithResource)
        };

        var message = GetResolver().ResolveMessage(info.InstanceType, info.Member.Name, attr);
        Assert.Equal(ConventionValidationMessages.Email_Required, message);
    }
}
