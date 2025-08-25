using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FluentAnnotationsValidator.Tests;

internal static partial class TestHelpers
{
    internal static FluentAnnotationsBuilder CreateBuilder(Action<ValidationBehaviorOptions>? configure = null) =>
        new ServiceCollection().AddFluentAnnotationsValidators(configure, typeof(TestLoginDto));

    internal static IFluentValidator<T> GetValidator<T>(Func<ValidationConfigurator, ValidationTypeConfigurator<T>>? configure = null,
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

    internal static bool BeComplexPassword(string password)
    {
        // A regular expression that checks for a complex password.
        // (?=.*[a-z])   - Must contain at least one lowercase letter.
        // (?=.*[A-Z])   - Must contain at least one uppercase letter.
        // (?=.*\d)      - Must contain at least one digit.
        // (?=.*[!@#$%^&*()_+=\[{\]};:\"'<,>.?/|\-`~]) - Must contain at least one non-alphanumeric character.
        // .             - Matches any character (except newline).

        var passwordRegex = ComplexPasswordRegex();

        return passwordRegex.IsMatch(password);
    }

    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).*$")]
    private static partial Regex ComplexPasswordRegex();
}
