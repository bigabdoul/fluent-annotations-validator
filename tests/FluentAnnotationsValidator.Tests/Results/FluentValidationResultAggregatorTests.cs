using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Results;
using static TestHelpers;

/// <summary>
/// The tests in this class are the exact same as the ones in 
/// <see cref="ValidationResultAggregatorTests"/>
/// except they use the FluentValidation-specific API.
/// </summary>
/// <remarks>
/// The <see cref="IFluentValidator{T}"/> interface is a wrapper 
/// around the <see cref="IFluentValidator{T}"/> defined in the 
/// FluentValidation API, and allows getting away from a direct
/// reference to the FluentValidation library.
/// </remarks>
public class FluentValidationResultAggregatorTests
{
    [Fact]
    public void Should_Run_WhenFluentConditionIsMet()
    {
        new ServiceCollection().AddFluentAnnotationsValidators()
            .UseFluentAnnotations()
                .For<TestLoginDto>()
            .Build();

        var dto = new TestLoginDto(Email: null!, Password: "Pass123", Role: "Admin");

        var validator = GetFluentValidator(builder =>
            builder.For<TestLoginDto>().When(x => x.Email, model => model.Role == "Admin"));

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_SkipMember_WhenConditionFails()
    {
        var dto = new TestLoginDto(Email: null!, Password: "Pass123", Role: "User");

        var validator = GetFluentValidator(builder =>
            builder.For<TestLoginDto>()
                .Rule(x => x.Email, FluentAnnotationsValidator.Configuration.RuleDefinitionBehavior.Preserve)
                //.Required()
                //.EmailAddress()
                //.When(m => m.Role == "Admin")
                .When(x => x.Email, model => model.Role == "Admin")
            );

        var result = validator.Validate(dto);

        //Assert.True(result.IsValid);
        //Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Email");
        result.IsValid.Should().BeTrue("Email is required only when the role is Admin.");
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(TestLoginDto.Email));
    }

    [Fact]
    public void Should_RunAttributes_WhenNoFluentRuleExists()
    {
        var dto = new TestLoginDto(Email: null!, Password: null!, Role: null);

        var validator = GetFluentValidator<TestLoginDto>();

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_AggregateAllAttributeFailures()
    {
        var dto = new TestLoginDto(Email: "", Password: "", Role: null);

        var validator = GetFluentValidator<TestLoginDto>();

        var result = validator.Validate(dto);
        var emailErrors = result.Errors.Where(e => e.PropertyName == "Email").ToList();

        Assert.Equal(2, emailErrors.Count); // [Required] + [EmailAddress]
    }

    [Fact]
    public void Should_EvaluateFluentAndAttributes_WhenConditionMet()
    {
        var dto = new TestLoginDto(Email: "", Password: "", Role: "Admin");

        var validator = GetFluentValidator(builder => builder
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

        var validator = GetFluentValidator<TestLoginDto>();

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }
}
