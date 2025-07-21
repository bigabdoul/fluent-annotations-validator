using FluentAnnotationsValidator.AspNetCore.Tests.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.AspNetCore.Tests;

internal static class TestHelpers
{
    internal static IServiceCollection CreateServices(Action<ValidationBehaviorOptions>? configure = null) =>
        new ServiceCollection().AddFluentAnnotationsValidators(configure, typeof(TestLoginDto));

    internal static IValidator<T> GetValidator<T>(Action<ValidationBehaviorOptions>? configure = null)
    {
        var services = CreateServices(configure);
        return services.BuildServiceProvider()
            .GetRequiredService<IValidator<T>>();
    }

    internal static IValidator<T> GetValidator<T>(this IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IValidator<T>>();

}
