using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;

using Models;
using Resources;
using static TestHelpers;

public class Resolver_PriorityTests
{
    private static ValidationMessageResolver GetResolver() => new(new GlobalRegistry(), new Mock<IStringLocalizerFactory>().Object);
    private static TestLoginDto NewTestLogin => new("user@example.com", "password");

    private class WrongMessages
    {
        public static string WrongKey => "Wrong!";
    }

    [Fact]
    public void Rule_Message_OverridesEverything()
    {
        var rule = new ValidationRule(message: "Override wins");

        var attr = new RequiredAttribute();
        var (member, instanceType) = CreateInfo<TestLoginDto>(x => x.Email);

        var msg = GetResolver().ResolveMessage(NewTestLogin, member.Name, attr, rule);
        Assert.Equal("Override wins", msg);
    }

    [Fact]
    public void Rule_ResourceKey_OverridesAttributeResource()
    {
        var rule = new ValidationRule(
            resourceKey: nameof(ValidationMessages.EmailRequired),
            resourceType: typeof(ValidationMessages)
        );

        var attr = new RequiredAttribute { ErrorMessageResourceName = nameof(WrongMessages.WrongKey), ErrorMessageResourceType = typeof(WrongMessages) };
        var (member, instanceType) = CreateInfo<TestLoginDto>(x => x.Email);
        var resolver = GetMessageResolver<ValidationMessages>(ValidationMessages.EmailRequired);

        // Act
        var msg = resolver.ResolveMessage(NewTestLogin, member.Name, attr, rule);

        // Assert
        msg.Should().NotBeNull();
        msg.Should().Be(ValidationMessages.EmailRequired);
    }

    [Fact]
    public void Attribute_Resource_OverridesConvention()
    {
        var attr = new RequiredAttribute
        {
            ErrorMessageResourceName = nameof(ValidationMessages.EmailRequired),
            ErrorMessageResourceType = typeof(ValidationMessages)
        };

        var (member, instanceType) = CreateInfo<TestLoginDtoWithResource>(x => x.Email);
        var resolver = GetMessageResolver<ValidationMessages>(ValidationMessages.EmailRequired);

        // Act
        var msg = resolver.ResolveMessage(new TestLoginDtoWithResource(), member.Name, attr);

        // Assert
        msg.Should().NotBeNull();
        msg.Should().Be(ValidationMessages.EmailRequired);
    }

    [Fact]
    public void Convention_Fallback_Used_WhenNoResourceSpecified()
    {
        var attr = new RequiredAttribute();
        var (member, instanceType) = CreateInfo<TestLoginDtoWithResource>(x => x.Email);
        ConventionValidationMessages.Culture = CultureInfo.CurrentCulture;
        var resolver = GetMessageResolver<ConventionValidationMessages>(ConventionValidationMessages.Email_Required);

        // Act
        var msg = resolver.ResolveMessage(new TestLoginDtoWithResource(), member.Name, attr);

        // Assert
        msg.Should().NotBeNull();
        msg.Should().Be(ConventionValidationMessages.Email_Required); // uses conventional key
    }

    [Fact]
    public void FallbackMessage_Used_WhenResourceFails()
    {
        var rule = new ValidationRule(
            resourceKey: "MissingKey",
            resourceType: typeof(WrongMessages),
            fallbackMessage: "Use this instead"
        );

        var attr = new RequiredAttribute();
        var (member, instanceType) = CreateInfo<TestLoginDto>(x => x.Email);
        var resolver = GetMessageResolver<WrongMessages>(null); // null means the resource was not found

        // Act
        var msg = resolver.ResolveMessage(NewTestLogin, member.Name, attr, rule);

        // Assert
        msg.Should().NotBeNull();
        msg.Should().Be("Use this instead");
    }

    private static (MemberInfo Member, Type InstanceType) CreateInfo<T>(Expression<Func<T, string?>> expr) =>
        (expr.GetMemberInfo(), typeof(T));
}
