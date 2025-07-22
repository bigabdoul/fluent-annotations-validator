using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages;

public class ValidationMessageResolverTests
{
    private readonly ValidationMessageResolver _resolver = new();

    [Fact]
    public void ResolveMessage_Inline_ReturnsFormattedMessage()
    {
        // Arrange
        var attr = new RequiredAttribute { ErrorMessage = "Field {0} is required" };
        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            TargetModelType = typeof(TestLoginDto)
        };

        // Act
        var message = _resolver.ResolveMessage(info, attr);

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

        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            TargetModelType = typeof(TestLoginDto)
        };

        var message = _resolver.ResolveMessage(info, attr);
        Assert.Equal("Email is required.", message);
    }

    [Fact]
    public void ResolveMessage_ConditionalRuleMetadata_WinsOverAttribute()
    {
        var attr = new RequiredAttribute();
        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            TargetModelType = typeof(TestLoginDto)
        };

        var rule = new ConditionalValidationRule(Predicate: null!, Message: "Overridden message");

        var message = _resolver.ResolveMessage(info, attr, rule);
        Assert.Equal("Overridden message", message);
    }

    [Fact]
    public void ResolveMessage_ResourceKey_FromRuleType_FormatsCorrectly()
    {
        var attr = new StringLengthAttribute(5);
        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!,
            TargetModelType = typeof(TestLoginDto)
        };

        var rule = new ConditionalValidationRule(Predicate: null!, 
            ResourceKey: nameof(ValidationMessages.PasswordRequired), 
            ResourceType: typeof(ValidationMessages));

        var message = _resolver.ResolveMessage(info, attr, rule);
        Assert.Equal("Password cannot be blank.", message);
    }

    [Fact]
    public void ResolveMessage_Fallback_UsesConventionBasedResolution()
    {
        var attr = new RequiredAttribute();
        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!,
            TargetModelType = typeof(TestLoginDtoWithResource)
        };

        var message = _resolver.ResolveMessage(info, attr);
        Assert.Equal("Email is required (convention).", message);
    }
}
