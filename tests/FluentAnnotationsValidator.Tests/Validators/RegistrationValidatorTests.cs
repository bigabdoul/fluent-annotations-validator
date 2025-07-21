using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Runtime.Validators;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using Microsoft.Extensions.Options;

namespace FluentAnnotationsValidator.Tests.Validators;

public class RegistrationValidatorTests
{
    private readonly DataAnnotationsValidator<TestRegistrationDto> _validator = 
        new(new ValidationMessageResolver(), CreateOptions());

    private static IOptions<ValidationBehaviorOptions> CreateOptions()
        => Options.Create(new ValidationBehaviorOptions());
    
    [Fact]
    public void ValidDto_ShouldPass()
    {
        var dto = new TestRegistrationDto
        {
            Email = "valid@example.com",
            Password = "secure123"
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void MissingEmail_ShouldFailWithLocalizedMessage()
    {
        var dto = new TestRegistrationDto
        {
            Email = "",
            Password = "secure123"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(dto.Email) &&
            e.ErrorMessage == ValidationMessages.Email_Required);
    }

    [Fact]
    public void InvalidEmailFormat_ShouldFail()
    {
        var dto = new TestRegistrationDto
        {
            Email = "not-an-email",
            Password = "secure123"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(dto.Email) &&
            (e.ErrorMessage.Contains("e-mail") || e.ErrorMessage.Contains("format")));
    }

    [Fact]
    public void MissingPassword_ShouldFailWithLocalizedMessage()
    {
        var dto = new TestRegistrationDto
        {
            Email = "user@example.com",
            Password = ""
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(dto.Password) &&
            e.ErrorMessage == ValidationMessages.Password_Required);
    }

    [Fact]
    public void ShortPassword_ShouldFailMinLength()
    {
        var dto = new TestRegistrationDto
        {
            Email = "user@example.com",
            Password = "123"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(dto.Password) &&
            (e.ErrorMessage.Contains("minimum") || e.ErrorMessage.Contains("length")));
    }

    [Fact]
    public void MultipleViolations_ShouldReportAllErrors()
    {
        var dto = new TestRegistrationDto
        {
            Email = "",     // 2 errors: email is required, format is invalid
            Password = "12" // 1 error: too short
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }
}

