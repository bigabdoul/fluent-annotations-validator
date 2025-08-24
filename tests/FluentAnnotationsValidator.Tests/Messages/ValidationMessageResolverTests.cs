using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentAssertions;
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
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;

        // Act
        var message = GetResolver().ResolveMessage(typeof(TestLoginDto), member.Name, attr);

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

        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;

        var message = GetResolver().ResolveMessage(typeof(TestLoginDto), member.Name, attr);
        Assert.Equal(ValidationMessages.EmailRequired, message);
    }

    [Fact]
    public void ResolveMessage_ConditionalRuleMetadata_WinsOverAttribute()
    {
        // Arrange
        var attr = new RequiredAttribute();
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;
        var rule = new ConditionalValidationRule(Predicate: null!, Message: "Overridden message");

        // Act
        var message = GetResolver().ResolveMessage(typeof(TestLoginDto), member.Name, attr, rule);

        // Assert
        Assert.Equal("Overridden message", message);
    }

    [Fact]
    public void ResolveMessage_ResourceKey_FromRuleType_FormatsCorrectly()
    {
        // Arrange
        var attr = new StringLengthAttribute(5);
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!;

        var rule = new ConditionalValidationRule(Predicate: null!, 
            ResourceKey: nameof(ValidationMessages.PasswordRequired), 
            ResourceType: typeof(ValidationMessages));

        // Act
        var message = GetResolver().ResolveMessage(typeof(TestLoginDto), member.Name, attr, rule);

        // Assert
        Assert.Equal(ValidationMessages.PasswordRequired, message);
    }

    [Fact]
    public void ResolveMessage_Fallback_UsesConventionBasedResolution()
    {
        // Arrange
        var resolver = GetResolver();
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;
        var attribute = new RequiredAttribute();

        var rule = new ConditionalValidationRule(Predicate: null!,
            ResourceKey: nameof(ConventionValidationMessages.Email_Required),
            ResourceType: typeof(ConventionValidationMessages));

        // Act
        // This is where you would call your message resolution method.
        var resolvedMessage = resolver.ResolveMessage(typeof(TestLoginDto), member.Name, attribute, rule);

        // Assert
        resolvedMessage.Should().NotBeNull();
        resolvedMessage.Should().Be(ConventionValidationMessages.Email_Required);
    }
}
