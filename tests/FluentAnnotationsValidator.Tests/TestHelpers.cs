using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
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

    internal static IStringLocalizerFactory MockStringLocalizerFactory<T>(string? localizedStringValue)
    {
        // 1. Mock the IStringLocalizer for the specific resource type
        var localizerMock = new Mock<IStringLocalizer>();
        localizerMock.Setup(l => l[It.IsAny<string>()])
                     .Returns<string>(s => new LocalizedString(s, localizedStringValue ?? string.Empty, resourceNotFound: localizedStringValue is null));

        // 2. Mock the IStringLocalizerFactory to return the localizer mock for the specific resource type
        var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
        localizerFactoryMock.Setup(f => f.Create(typeof(T)))
                             .Returns(localizerMock.Object);

        return localizerFactoryMock.Object;
    }

    internal static ValidationMessageResolver GetMessageResolver<T>(string? localizedStringValue) =>
        new(new ValidationBehaviorOptions(), MockStringLocalizerFactory<T>(localizedStringValue));
}
