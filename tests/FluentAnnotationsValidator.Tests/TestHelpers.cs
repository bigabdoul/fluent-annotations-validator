using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using System.Text.RegularExpressions;

namespace FluentAnnotationsValidator.Tests;

using Models;

internal static partial class TestHelpers
{
    internal static FluentAnnotationsBuilder CreateBuilder(Action<ValidationRuleGroupRegistry>? configure = null) =>
        new ServiceCollection().AddFluentAnnotationsValidators(new ConfigurationOptions
        {
            ConfigureRegistry = configure,
            ExtraValidatableTypesFactory = () => [typeof(TestLoginDto)],
        });

    internal static IFluentValidator<T> GetValidator<T>(Func<FluentTypeValidatorRoot, FluentTypeValidator<T>>? configure = null)
    {
        var builder = CreateBuilder();
        var services = builder.Services;
        var fluent = builder.UseFluentAnnotations();

        if (configure != null)
        {
            configure(fluent).Build();
        }

        return services.BuildServiceProvider().GetRequiredService<IFluentValidator<T>>();
    }

    internal static IFluentValidator<T> GetFluentValidator<T>(Func<FluentTypeValidatorRoot, FluentTypeValidator<T>>? configure = null)
    {
        var builder = CreateBuilder();
        var services = builder.Services;
        var fluent = builder.UseFluentAnnotations();
        if (configure != null)
        {
            configure(fluent).Build();
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
        new(new GlobalRegistry(), MockStringLocalizerFactory<T>(localizedStringValue));

    internal static TestLoginDto NewTestLoginDto => new(Email: "user@example.com", Password: "weak-Password-");
}
