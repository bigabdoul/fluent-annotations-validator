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
            .AddFluentAnnotationsValidators(typeof(TestLoginDto))
            .BuildServiceProvider();

        var validator = services.GetService<IValidator<TestLoginDto>>();
        Assert.NotNull(validator);
    }
}

