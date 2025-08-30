using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Results;
using static TestHelpers;

[Collection("ValidationIsolation")]
public class ValidationResultAggregatorTests
{
    [Fact]
    public void Should_Run_WhenFluentConditionIsMet()
    {
        // init DTO with a missing email
        var dto = new TestLoginDto(Email: null!, Password: "Pass123", Role: "Admin");

        var validator = GetValidator(builder =>
            builder.For<TestLoginDto>().When(x => x.Email, model => model.Role == "Admin"));

        var result = validator.Validate(dto);

        Assert.False(result.IsValid); // the email is required
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_SkipMember_WhenConditionFails()
    {
        // Missing email address
        var dto = new TestLoginDto(Email: null!, Password: "Pass123", Role: "User");

        var validator = GetValidator(builder =>
            builder.For<TestLoginDto>()
                // validation happens only if role is "Admin";
                // since the DTO has a "User" role, email
                // validation is skipped and the result is valid
                .When(x => x.Email, model => model.Role == "Admin")
        );

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_RunAttributes_WhenNoFluentRuleExists()
    {
        var dto = new TestLoginDto(Email: null!, Password: null!, Role: null);

        var validator = GetValidator<TestLoginDto>();

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_AggregateAllAttributeFailures()
    {
        var dto = new TestLoginDto(Email: "", Password: "", Role: null);

        var validator = GetValidator<TestLoginDto>();

        var result = validator.Validate(dto);
        var emailErrors = result.Errors.Where(e => e.PropertyName == "Email").ToList();

        Assert.Equal(2, emailErrors.Count); // [Required] + [EmailAddress]
    }

    [Fact]
    public void Should_EvaluateFluentAndAttributes_WhenConditionMet()
    {
        var dto = new TestLoginDto(Email: "", Password: "", Role: "Admin");

        var validator = GetValidator(builder => builder
            .For<TestLoginDto>()
                .When(x => x.Email, m => m.Role == "Admin")
                .Localized("Email_Required")
                .UseFallbackMessage("Email required by admin.")
        );

        // Act
        var result = validator.Validate(dto);
        var errorCount = result.Errors.Count(e => e.PropertyName == "Email");

        Assert.True(errorCount >= 2);
    }

    [Fact]
    public void Should_RespectAttributesOnRecordCtorParameters()
    {
        var dto = new TestLoginDto(Email: null!, Password: null!, Role: null);

        var validator = GetValidator<TestLoginDto>();

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

}

[CollectionDefinition("ValidationIsolation")]
public class IsolationCollection : ICollectionFixture<object> { }
