using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Tests.Assertions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Configuration;
using static TestHelpers;

public class ValidationTypeConfiguratorTests
{
    private readonly IServiceCollection _services = CreateServices();
    private readonly ValidationConfigurator _parent;
    private readonly ValidationTypeConfigurator<TestLoginDto> _validator;

    public ValidationTypeConfiguratorTests()
    {
        _parent = new(_services);
        _validator = new ValidationTypeConfigurator<TestLoginDto>(_parent);
    }

    [Fact]
    public void WithValidationResource_SetsResourceTypeCorrectly()
    {
        // Arrange
        var configurator = new ValidationTypeConfigurator<TestLoginDto>(_parent);

        // Act
        var result = configurator.WithValidationResource<ValidationMessages>();

        // Assert
        var field = typeof(ValidationTypeConfigurator<TestLoginDto>)
            .GetField("_resourceType", BindingFlags.Instance | BindingFlags.NonPublic);

        var value = field?.GetValue(configurator);
        Assert.Equal(typeof(ValidationMessages), value);
        Assert.Same(configurator, result); // fluent chaining
    }

    [Fact]
    public void Build_PersistsResourceTypeOnRules()
    {
        // Arrange
        var configurator = new ValidationTypeConfigurator<TestLoginDto>(_parent)
            .WithValidationResource<ValidationMessages>()
            .When(x => x.Email, dto => string.IsNullOrWhiteSpace(dto.Email));

        // Act
        configurator.Build();

        var resolved = ResolveBehaviorOptions();
        _ = resolved.TryGet<TestLoginDto>(x => x.Email, out var rule);

        // Assert
        Assert.Multiple
        (
            () => Assert.NotNull(rule),
            () => Assert.Equal(typeof(ValidationMessages), rule!.ResourceType)
        );
    }

    [Fact]
    public void When_AddsConditionalRule()
    {
        _validator.When(x => x.Email, dto => dto.Role == "Admin").Build();

        // Simulate DI resolution
        var resolved = ResolveBehaviorOptions();

        Assert.True(resolved.ContainsKey<TestLoginDto>(x => x.Email));
    }

    [Fact]
    public void And_IsAliasForWhen()
    {
        _validator.And(x => x.Password, dto => dto.Role != "Guest").Build();

        var resolved = ResolveBehaviorOptions();

        Assert.True(resolved.ContainsKey<TestLoginDto>(x => x.Password));
    }

    [Fact]
    public void Except_DisablesValidation()
    {
        _validator.Except(x => x.Role).Build();
        var rule = GetRule(x => x.Role);

        rule.ShouldNotMatch(predicateArg: new TestLoginDto("a", "b", "c"));
    }

    [Fact]
    public void AlwaysValidate_AlwaysReturnsTrue()
    {
        _validator.AlwaysValidate(x => x.Password).Build();
        var rule = GetRule(x => x.Password);

        rule.ShouldMatch(predicateArg: new TestLoginDto("a", "b", "c"));
    }

    [Fact]
    public void WithMessage_AttachesMessage()
    {
        _validator.When(x => x.Email, dto => true)
            .WithMessage("Custom error")
            .Build();

        var rule = GetRule(x => x.Email);

        rule.ShouldMatch(expectedMessage: "Custom error");
    }

    [Fact]
    public void WithKey_AttachesValidationKey()
    {
        _validator.When(x => x.Email, dto => true)
            .WithKey("Email.AdminRequired")
            .Build();

        var rule = GetRule(x => x.Email);

        rule.ShouldMatch(expectedKey: "Email.AdminRequired");
    }

    [Fact]
    public void Localized_AttachesResourceKey()
    {
        _validator.When(x => x.Email, dto => true)
            .Localized("Admin_Email_Required")
            .Build();

        var rule = GetRule(x => x.Email);

        rule.ShouldMatch(expectedResource: "Admin_Email_Required");
    }

    [Fact]
    public void For_TransitionsToNextConfigurator()
    {
        var next = _validator.For<TestRegistrationDto>();
        Assert.IsType<ValidationTypeConfigurator<TestRegistrationDto>>(next);
    }

    [Fact]
    public void Build_RegistersMultipleRules()
    {
        _validator.When(x => x.Email, dto => true)
                  .WithMessage("Must validate email")
               .And(x => x.Password, dto => true)
                  .WithMessage("Must validate password")
               .Build();

        var emailr = GetRule(x => x.Email);
        var passwr = GetRule(x => x.Password);

        emailr.ShouldMatch(expectedMessage: "Must validate email");
        passwr.ShouldMatch(expectedMessage: "Must validate password");
    }

    [Fact]
    public void MessageChaining_PersistsUntilBuild()
    {
        _validator.When(x => x.Email, dto => true)
            .WithMessage("Required")
            .WithKey("Email.Required")
            .Localized("Email_Required_Localized")
            .Build();

        var rule = GetRule(x => x.Email);

        rule.ShouldMatch(expectedMessage: "Required",
            expectedKey: "Email.Required",
            expectedResource: "Email_Required_Localized");
    }

    [Fact]
    public void Build_RegistersAllBufferedRules()
    {
        _validator.When(x => x.Email, dto => true)
            .WithMessage("Email required")
            .Build();

        var rule = GetRule(x => x.Email);

        rule.ShouldMatch(expectedMessage: "Email required");
    }

    [Fact]
    public void Metadata_IsAttachedCorrectly()
    {
        _validator.When(x => x.Email, dto => true)
            .WithMessage("Custom message")
            .WithKey("Email.Required")
            .Localized("Email_Required")
            .Build();

        var rule = GetRule(x => x.Email);

        rule.ShouldMatch(expectedMessage: "Custom message",
            expectedKey: "Email.Required",
            expectedResource: "Email_Required");
    }

    private ConditionalValidationRule GetRule(Expression<Func<TestLoginDto, string?>> property)
    {
        _parent.Build();
        var resolved = ResolveBehaviorOptions();
        return resolved.Get(property);
    }

    private ValidationBehaviorOptions ResolveBehaviorOptions()
    {
        var provider = _services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IOptions<ValidationBehaviorOptions>>().Value;
        return resolved;
    }
}
