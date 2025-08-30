using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages;
using static TestHelpers;

public class ValidationMessageResolverTests
{
    private static ValidationMessageResolver GetResolver() =>
        new(new ValidationBehaviorOptions(), new Mock<IStringLocalizerFactory>().Object);

    public ValidationMessageResolverTests()
    {
        ValidationMessages.Culture = ConventionValidationMessages.Culture = Thread.CurrentThread.CurrentCulture;
    }

    [Fact]
    public void ResolveMessage_Inline_ReturnsFormattedMessage()
    {
        // Arrange
        var attr = new RequiredAttribute { ErrorMessage = "Field {0} is required" };
        var resolver = GetResolver();

        // Act
        var message = resolver.ResolveMessage(typeof(TestLoginDto), nameof(TestLoginDto.Email), attr);

        // Assert
        message.Should().NotBeNull();
        message.Should().Be("Field Email is required");
    }

    [Fact]
    public void ResolveMessage_AttributeResource_ReturnsFromResourceType()
    {
        // Arrange
        var attr = new RequiredAttribute
        {
            ErrorMessageResourceName = nameof(ValidationMessages.EmailRequired),
            ErrorMessageResourceType = typeof(ValidationMessages)
        };

        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;
        var resolver = GetResolver();

        // Act
        var message = resolver.ResolveMessage(typeof(TestLoginDto), member.Name, attr);

        // Assert
        message.Should().NotBeNull();
        message.Should().Be(ValidationMessages.EmailRequired);
    }

    [Fact]
    public void ResolveMessage_ConditionalRuleMetadata_WinsOverAttribute()
    {
        // Arrange
        var attr = new RequiredAttribute();
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;
        var rule = new ConditionalValidationRule(predicate: null!, message: "Overridden message");
        var resolver = GetResolver();

        // Act
        var message = resolver.ResolveMessage(typeof(TestLoginDto), member.Name, attr, rule);

        // Assert
        Assert.Equal("Overridden message", message);
    }

    [Fact]
    public void ResolveMessage_ResourceKey_FromRuleType_FormatsCorrectly()
    {
        // Arrange
        var attr = new StringLengthAttribute(5);
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!;

        var rule = new ConditionalValidationRule(predicate: null!,
            resourceKey: nameof(ValidationMessages.PasswordRequired),
            resourceType: typeof(ValidationMessages));

        var resolver = GetMessageResolver<ValidationMessages>(ValidationMessages.PasswordRequired);

        // Act
        var message = resolver.ResolveMessage(typeof(TestLoginDto), member.Name, attr, rule);

        // Assert
        message.Should().NotBeNull();
        message.Should().Be(ValidationMessages.PasswordRequired);
    }

    [Fact]
    public void ResolveMessage_Fallback_UsesConventionBasedResolution()
    {
        // Arrange
        var member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Email))!;
        var attribute = new RequiredAttribute();

        var rule = new ConditionalValidationRule(predicate: null!,
            resourceKey: nameof(ConventionValidationMessages.Email_Required),
            resourceType: typeof(ConventionValidationMessages));

        var resolver = GetMessageResolver<ConventionValidationMessages>(ConventionValidationMessages.Email_Required);

        // Act
        // This is where you would call your message resolution method.
        var resolvedMessage = resolver.ResolveMessage(typeof(TestLoginDto), member.Name, attribute, rule);

        // Assert
        resolvedMessage.Should().NotBeNull();
        resolvedMessage.Should().Be(ConventionValidationMessages.Email_Required);
    }
}
