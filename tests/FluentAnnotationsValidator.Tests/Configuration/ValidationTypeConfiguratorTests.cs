using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Tests.Configuration;

public class ValidationTypeConfiguratorTests
{
    private static ValidationTypeConfigurator<TestLoginDto> GetConfigurator()
    {
        return new ServiceCollection()
            .AddFluentAnnotationsValidators()
            .UseFluentAnnotations()
            .For<TestLoginDto>();
    }

    [Fact]
    public void WithValidationResource_SetsResourceTypeCorrectly()
    {
        // Arrange
        var configurator = GetConfigurator();
        var propertyName = nameof(ValidationTypeConfigurator<TestLoginDto>.ValidationResourceType);
        var property = typeof(ValidationTypeConfigurator<TestLoginDto>).GetProperty(propertyName);

        // Act
        var result = configurator.WithValidationResource<ValidationMessages>();

        // Assert
        property.Should().NotBeNull();

        var propertyValue = property.GetValue(configurator);

        propertyValue.Should().NotBeNull();
        propertyValue.Should().BeEquivalentTo(typeof(ValidationMessages));

        configurator.Should().BeSameAs(result);
    }

    [Fact]
    public void Build_PersistsResourceTypeOnRules()
    {
        // Arrange
        var configurator = GetConfigurator()
            .WithValidationResource<ValidationMessages>()
            .When(x => x.Email, dto => string.IsNullOrWhiteSpace(dto.Email));

        // Act
        configurator.Build();

        _ = configurator.Options.TryGetRules<TestLoginDto>(x => x.Email, out var rules);

        // Assert
        rules.Should().NotBeEmpty();
        rules.Should().Contain(r => r.ResourceType == typeof(ValidationMessages));
    }

    [Fact]
    public void When_AddsConditionalRule()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => dto.Role == "Admin").Build();

        configurator.Options
            .Contains<TestLoginDto>(x => x.Email)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void And_IsAliasForWhen()
    {
        var configurator = GetConfigurator();
        configurator.And(x => x.Password, dto => dto.Role != "Guest").Build();

        // Assert
        configurator.Options
            .Contains<TestLoginDto>(x => x.Password)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void AlwaysValidate_AlwaysReturnsTrue()
    {
        var configurator = GetConfigurator();
        configurator.AlwaysValidate(x => x.Password).Build();
        var rule = GetRule(x => x.Password, configurator.Options);

        rule.Predicate.Should().NotBeNull();
        rule.Predicate.Target.Should().NotBeNull();
        rule.Predicate.Invoke(new TestLoginDto("a", "b", "c")).Should().BeTrue();
    }

    [Fact]
    public void WithMessage_AttachesMessage()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .WithMessage("Custom error")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.Message.Should().NotBeNull();
        rule.Message.Should().Match("Custom error");
    }

    [Fact]
    public void WithKey_AttachesValidationKey()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .WithKey("Email.AdminRequired")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.Key.Should().NotBeNull();
        rule.Key.Should().Match("Email.AdminRequired");
    }

    [Fact]
    public void Localized_AttachesResourceKey()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .Localized("Admin_Email_Required")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.ResourceKey.Should().NotBeNull();
        rule.ResourceKey.Should().Match("Admin_Email_Required");
    }

    [Fact]
    public void For_TransitionsToNextConfigurator()
    {
        var next = GetConfigurator().For<TestRegistrationDto>();
        Assert.IsType<ValidationTypeConfigurator<TestRegistrationDto>>(next);
    }

    [Fact]
    public void Build_RegistersMultipleRules()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
                  .WithMessage("Must validate email")
               .And(x => x.Password, dto => true)
                  .WithMessage("Must validate password")
               .Build();

        var emailr = GetRule(x => x.Email, configurator.Options);
        var passwr = GetRule(x => x.Password, configurator.Options);

        emailr.Message.Should().NotBeNull();
        emailr.Message.Should().Match("Must validate email");

        passwr.Message.Should().NotBeNull();
        passwr.Message.Should().Match("Must validate password");
    }

    [Fact]
    public void MessageChaining_PersistsUntilBuild()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .WithMessage("Required")
            .WithKey("Email.Required")
            .Localized("Email_Required_Localized")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.Message.Should().NotBeNull();
        rule.Message.Should().Match("Required");

        rule.Key.Should().NotBeNull();
        rule.Key.Should().Match("Email.Required");

        rule.ResourceKey.Should().NotBeNull();
        rule.ResourceKey.Should().Match("Email_Required_Localized");
    }

    [Fact]
    public void Build_RegistersAllBufferedRules()
    {
        var configurator = GetConfigurator();

        configurator.When(x => x.Email, dto => true)
            .WithMessage("Email required")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.Message.Should().NotBeNull();
        rule.Message.Should().Match("Email required");
    }

    [Fact]
    public void Metadata_IsAttachedCorrectly()
    {
        var configurator = GetConfigurator();

        configurator.When(x => x.Email, dto => true)
            .WithMessage("Custom message")
            .WithKey("Email.Required")
            .Localized("Email_Required")
        .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.Message.Should().NotBeNull();
        rule.Message.Should().Match("Custom message");

        rule.Key.Should().NotBeNull();
        rule.Key.Should().Match("Email.Required");

        rule.ResourceKey.Should().NotBeNull();
        rule.ResourceKey.Should().Match("Email_Required");
    }

    private static ConditionalValidationRule GetRule(Expression<Func<TestLoginDto, string?>> property, ValidationBehaviorOptions options)
    {
        var rules = options.GetRules(property, rule => !rule.HasAttribute);
        return rules[0];
    }
}
