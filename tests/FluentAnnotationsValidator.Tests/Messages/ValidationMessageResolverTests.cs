using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Messages;
using static TestHelpers;

public class ValidationMessageResolverTests
{
    public ValidationMessageResolverTests()
    {
        ValidationMessages.Culture = Thread.CurrentThread.CurrentUICulture;
    }

    [Fact]
    public void ResolveMessage_Inline_ReturnsFormattedMessage()
    {
        // Arrange
        var attr = new RequiredAttribute { ErrorMessage = "Field {0} is required" };
        var resolver = GetMessageResolver();

        // Act
        var message = resolver.ResolveMessage<TestLoginDto>(x => x.Email, attr);

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

        var resolver = GetMessageResolver();

        // Act
        var message = resolver.ResolveMessage<TestLoginDto>(x => x.Email, attr);

        Assert.Equal(ValidationMessages.EmailRequired, message);
    }

    [Fact]
    public void ResolveMessage_ConditionalRuleMetadata_WinsOverAttribute()
    {
        var rule = new ConditionalValidationRule(Predicate: null!, Message: "Overridden message");
        var resolver = GetMessageResolver();

        // Act
        var message = resolver.ResolveMessage<TestLoginDto>(x => x.Email, new RequiredAttribute(), rule);

        Assert.Equal("Overridden message", message);
    }

    [Fact]
    public void ResolveMessage_ResourceKey_FromRuleType_FormatsCorrectly()
    {
        var rule = new ConditionalValidationRule(Predicate: null!, 
            ResourceKey: nameof(ValidationMessages.PasswordRequired), 
            ResourceType: typeof(ValidationMessages)
        );
        var resolver = GetMessageResolver();

        // Act
        var message = resolver.ResolveMessage<TestLoginDto>(x => x.Password, new StringLengthAttribute(5), rule);

        Assert.Equal(ValidationMessages.PasswordRequired, message);
    }

    [Fact]
    public void ResolveMessage_Fallback_UsesConventionBasedResolution()
    {
        var resolver = GetMessageResolver();

        // Act
        var message = resolver.ResolveMessage<TestLoginDtoWithResource>(x => x.Email, new RequiredAttribute());

        Assert.Equal(ConventionValidationMessages.Email_Required, message);
    }
}
