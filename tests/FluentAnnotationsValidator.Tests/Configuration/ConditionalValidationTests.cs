using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Configuration;
using static TestHelpers;

public class ConditionalValidationTests
{
    [Fact]
    public async Task Validation_Should_SkipProperty_WhenConditionIsFalse()
    {
        var validator = GetValidator(builder => builder
            .For<TestLoginDto>()
                .When(x => x.Email, model => model.Role == "Admin")
        );

        // Email is null, but Role ≠ Admin
        var dto = new TestLoginDto(Email: null!, Password: "PasswordHere", Role: "User");

        // Act
        var result = await validator.ValidateAsync(dto);

        Assert.True(result.IsValid); // Email not validated
    }

    [Fact]
    public async Task Validation_Should_Apply_WhenNoConditionConfigured()
    {
        var validator = CreateBuilder().Services
            .BuildServiceProvider()
            .GetRequiredService<IFluentValidator<TestLoginDto>>();

        // Email and Password still required
        var dto = new TestLoginDto(null!, string.Empty);

        // Act
        var result = await validator.ValidateAsync(dto);

        Assert.Multiple
        (
            () => Assert.False(result.IsValid),
            () => Assert.Contains(result.Errors, e => e.PropertyName == nameof(TestLoginDto.Email)),
            () => Assert.Contains(result.Errors, e => e.PropertyName == nameof(TestLoginDto.Password))
        );
    }
}