using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests;

public class DIRegistrationTests
{
    [Fact]
    public void Should_ResolveValidatorForAnnotatedType()
    {
        var provider = TestHelpers.CreateBuilder().Services.BuildServiceProvider();
        var validator = provider.GetService<IFluentValidator<TestLoginDto>>();
        Assert.NotNull(validator);
    }
}

