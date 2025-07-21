using FluentAnnotationsValidator.AspNetCore.Tests.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.AspNetCore.Tests;

public class DIRegistrationTests
{
    [Fact]
    public void Should_ResolveValidatorForAnnotatedType()
    {
        var services = new ServiceCollection()
            .AddFluentAnnotationsValidators(typeof(LoginDto))
            .BuildServiceProvider();

        var validator = services.GetService<IValidator<LoginDto>>();
        Assert.NotNull(validator);
    }
}

