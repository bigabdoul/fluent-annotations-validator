using FluentAnnotationsValidator.Tests.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests;

public class DIRegistrationTests
{
    [Fact]
    public void Should_ResolveValidatorForAnnotatedType()
    {
        var provider = TestHelpers.CreateBuilder().Services.BuildServiceProvider();
        var validator = provider.GetService<IValidator<TestLoginDto>>();
        Assert.NotNull(validator);
    }
}

