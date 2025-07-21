using FluentAnnotationsValidator.AspNetCore.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.AspNetCore.Tests;
using static TestHelpers;

public class ConditionalValidationTests
{
    [Fact]
    public async Task Validation_Should_SkipProperty_WhenConditionIsFalse()
    {
        var services = CreateServices();
        services.UseFluentAnnotations()
            .For<LoginDto>()
                .When(x => x.Email, model => model.Role == "Admin")
            .Build();

        var validator = services.BuildServiceProvider().GetValidator<LoginDto>();

        var dto = new LoginDto(null!, "PasswordHere", Role: "User"); // Email is null, but Role ≠ Admin
        var result = await validator.ValidateAsync(dto);

        Assert.True(result.IsValid); // Email not validated
    }

    [Fact]
    public async Task Validation_Should_Apply_WhenNoConditionConfigured()
    {
        var validator = GetValidator<LoginDto>();

        var dto = new LoginDto(null!, string.Empty); // Email and Password still required
        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginDto.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginDto.Password));
    }
}