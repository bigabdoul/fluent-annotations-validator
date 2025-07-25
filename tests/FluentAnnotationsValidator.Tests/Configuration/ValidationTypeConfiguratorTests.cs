using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Tests.Assertions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Tests.Configuration;
public class ValidationTypeConfiguratorTests
{
    private static ValidationTypeConfigurator<TestLoginDto> GetConfigurator()
    {
        var options = new ValidationBehaviorOptions();
        var validationConfigurator = new ValidationConfigurator(options);
        return new(validationConfigurator, options);
    }

    [Fact]
    public void WithValidationResource_SetsResourceTypeCorrectly()
    {
        // Arrange
        var configurator = GetConfigurator();

        // Act
        var result = configurator.WithValidationResource<ValidationMessages>();

        // Assert
        var property = typeof(ValidationTypeConfigurator<TestLoginDto>).GetProperty("ValidationResourceType");

        var value = property?.GetValue(configurator);
        Assert.Equal(typeof(ValidationMessages), value);
        Assert.Same(configurator, result); // fluent chaining
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
        Assert.Multiple
        (
            () => Assert.NotEmpty(rules),
            () => Assert.Contains(rules, r => r.ResourceType == typeof(ValidationMessages)));
    }

    [Fact]
    public void When_AddsConditionalRule()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => dto.Role == "Admin").Build();

        Assert.True(configurator.Options.Contains<TestLoginDto>(x => x.Email));
    }

    [Fact]
    public void And_IsAliasForWhen()
    {
        var configurator = GetConfigurator();
        configurator.And(x => x.Password, dto => dto.Role != "Guest").Build();

        Assert.True(configurator.Options.Contains<TestLoginDto>(x => x.Password));
    }

    [Fact]
    public void AlwaysValidate_AlwaysReturnsTrue()
    {
        var configurator = GetConfigurator();
        configurator.AlwaysValidate(x => x.Password).Build();
        var rule = GetRule(x => x.Password, configurator.Options);

        rule.ShouldMatch(predicateArg: new TestLoginDto("a", "b", "c"));
    }

    [Fact]
    public void WithMessage_AttachesMessage()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .WithMessage("Custom error")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.ShouldMatch(expectedMessage: "Custom error");
    }

    [Fact]
    public void WithKey_AttachesValidationKey()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .WithKey("Email.AdminRequired")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.ShouldMatch(expectedKey: "Email.AdminRequired");
    }

    [Fact]
    public void Localized_AttachesResourceKey()
    {
        var configurator = GetConfigurator();
        configurator.When(x => x.Email, dto => true)
            .Localized("Admin_Email_Required")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.ShouldMatch(expectedResource: "Admin_Email_Required");
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

        emailr.ShouldMatch(expectedMessage: "Must validate email");
        passwr.ShouldMatch(expectedMessage: "Must validate password");
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

        rule.ShouldMatch(expectedMessage: "Required",
            expectedKey: "Email.Required",
            expectedResource: "Email_Required_Localized");
    }

    [Fact]
    public void Build_RegistersAllBufferedRules()
    {
        var configurator = GetConfigurator();

        configurator.When(x => x.Email, dto => true)
            .WithMessage("Email required")
            .Build();

        var rule = GetRule(x => x.Email, configurator.Options);

        rule.ShouldMatch(expectedMessage: "Email required");
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

        rule.ShouldMatch(expectedMessage: "Custom message",
            expectedKey: "Email.Required",
            expectedResource: "Email_Required");
    }

    private static ConditionalValidationRule GetRule(Expression<Func<TestLoginDto, string?>> property, ValidationBehaviorOptions options)
    {
        var rules = options.GetRules(property, rule => !rule.HasAttribute);
        return rules[0];
    }
}
