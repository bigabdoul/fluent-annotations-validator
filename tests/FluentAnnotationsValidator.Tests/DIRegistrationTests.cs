using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests;

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

