using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace FluentAnnotationsValidator.Tests;

internal static class TestHelpers
{
    internal static FluentAnnotationsBuilder CreateBuilder(Action<ValidationBehaviorOptions>? configure = null) =>
        new ServiceCollection().AddFluentAnnotationsValidators(configure, typeof(TestLoginDto));

    internal static IValidator<T> GetValidator<T>(Func<ValidationConfigurator, ValidationTypeConfigurator<T>>? configure = null,
        [CallerMemberName] string? testName = null)
    {
        var builder = CreateBuilder(options =>
        {
            options.CurrentTestName = testName;
        });
        var services = builder.Services;
        var fluent = builder.UseFluentAnnotations();
        if (configure != null)
        {
            configure(fluent).Build();
        }
        else
        {
            fluent.Build();
        }
        return services.BuildServiceProvider().GetRequiredService<IValidator<T>>();
    }

    internal static IFluentValidator<T> GetFluentValidator<T>(Func<ValidationConfigurator, ValidationTypeConfigurator<T>>? configure = null,
        [CallerMemberName] string? testName = null)
    {
        var builder = CreateBuilder(options =>
        {
            options.CurrentTestName = testName;
        });
        var services = builder.Services;
        var fluent = builder.UseFluentAnnotations();
        if (configure != null)
        {
            configure(fluent).Build();
        }
        else
        {
            fluent.Build();
        }
        return services.BuildServiceProvider().GetRequiredService<IFluentValidator<T>>();
    }
}
