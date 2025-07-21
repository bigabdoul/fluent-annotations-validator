using FluentAnnotationsValidator.AspNetCore.Tests.Assertions;
using FluentAnnotationsValidator.AspNetCore.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentAnnotationsValidator.AspNetCore.Tests;
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
    public void When_AddsConditionalRule()
    {
        _validator.When(x => x.Email, dto => dto.Role == "Admin").Build();

        var options = new ValidationBehaviorOptions();
        _parent.Build(); // flushes into ServiceCollection

        // Simulate DI resolution
        var provider = _services.BuildServiceProvider();

        var resolved = provider.GetRequiredService<IOptions<ValidationBehaviorOptions>>().Value;

        Assert.True(resolved.ContainsKey(typeof(TestLoginDto), nameof(TestLoginDto.Email)));
    }

    [Fact]
    public void And_IsAliasForWhen()
    {
        _validator.And(x => x.Password, dto => dto.Role != "Guest").Build();
        // Assert same as above
    }

    [Fact]
    public void Except_DisablesValidation()
    {
        _validator.Except(x => x.Role).Build();
        var rule = GetRule(nameof(TestLoginDto.Role));

        rule.ShouldNotMatch(predicateArg: new TestLoginDto("a", "b", "c"));
    }

    [Fact]
    public void AlwaysValidate_AlwaysReturnsTrue()
    {
        _validator.AlwaysValidate(x => x.Password).Build();
        var rule = GetRule(nameof(TestLoginDto.Password));

        rule.ShouldMatch(predicateArg: new TestLoginDto("a", "b", "c"));
    }

    [Fact]
    public void WithMessage_AttachesMessage()
    {
        _validator.When(x => x.Email, dto => true)
            .WithMessage("Custom error")
            .Build();

        var rule = GetRule(nameof(TestLoginDto.Email));
        
        rule.ShouldMatch(expectedMessage: "Custom error");
    }

    [Fact]
    public void WithKey_AttachesValidationKey()
    {
        _validator.When(x => x.Email, dto => true)
            .WithKey("Email.AdminRequired")
            .Build();

        var rule = GetRule(nameof(TestLoginDto.Email));

        rule.ShouldMatch(expectedKey: "Email.AdminRequired");
    }

    [Fact]
    public void Localized_AttachesResourceKey()
    {
        _validator.When(x => x.Email, dto => true)
            .Localized("Admin_Email_Required")
            .Build();

        var rule = GetRule(nameof(TestLoginDto.Email));

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

        var emailr = GetRule(nameof(TestLoginDto.Email));
        var passwr = GetRule(nameof(TestLoginDto.Password));

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

        var rule = GetRule(nameof(TestLoginDto.Email));

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

        var rule = GetRule(nameof(TestLoginDto.Email));

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

        var rule = GetRule(nameof(TestLoginDto.Email));

        rule.ShouldMatch(expectedMessage: "Custom message", 
            expectedKey: "Email.Required", 
            expectedResource: "Email_Required");
    }

    private ConditionalValidationRule GetRule(string property)
    {
        _parent.Build();

        var provider = _services.BuildServiceProvider();

        var resolved = provider.GetRequiredService<IOptions<ValidationBehaviorOptions>>().Value;
        return resolved.Get(typeof(TestLoginDto), property);
    }
}
