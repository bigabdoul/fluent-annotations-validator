using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FluentAnnotationsValidator.Tests;

internal static class TestHelpers
{
    internal static FluentAnnotationsBuilder CreateBuilder(Action<ValidationBehaviorOptions>? configure = null) =>
        new ServiceCollection().AddFluentAnnotationsValidators(configure, typeof(TestLoginDto));

    /// <summary>
    /// Creates a <see cref="FluentAnnotationsBuilder"/>, configures it by setting the 
    /// <see cref="ValidationBehaviorOptions.CurrentTestName"/> property value to the
    /// test that called this method, proceeds to either configure the builder using
    /// the <paramref name="configure"/> action, if any, or builds it.
    /// Finally, it returns a resolved <see cref="IValidator{T}"/> service.
    /// </summary>
    /// <typeparam name="T">The type of the model to validate.</typeparam>
    /// <param name="configure">A function used to configure the created builder.</param>
    /// <param name="testName">
    /// The name of the test that called this method.
    /// If not specified, the name is inferred from the method caller.</param>
    /// <returns></returns>
    internal static IValidator<T> GetValidator<T>(Func<ValidationConfigurator, ValidationTypeConfigurator<T>>? configure = null,
        [CallerMemberName] string? testName = null)
    {
        var builder = CreateBuilder(configure: options => options.CurrentTestName = testName);
        var validationConfigurator = builder.UseFluentAnnotations();

        if (configure != null)
        {
            configure(validationConfigurator).Build();
        }
        else
        {
            validationConfigurator.Build();
        }

        return builder.Services.BuildServiceProvider().GetRequiredService<IValidator<T>>();
    }

    internal static IValidator<T> GetValidator<T>(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IValidator<T>>();
    }

    internal static string? GetValidationMessage<T, TAttribute>(Expression<Func<T, string?>> expression, string cultureName)
        where TAttribute : ValidationAttribute
        => GetValidationMessage<T, TAttribute>(expression, CultureInfo.GetCultureInfo(cultureName));

    internal static string? GetValidationMessage<T, TAttribute>(Expression<Func<T, string?>> expression, CultureInfo culture)
        where TAttribute : ValidationAttribute
    {
        var member = expression.GetMemberInfo();
        string attributeShortName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var resourceKey = $"{member.Name}_{attributeShortName}";
        return ValidationMessages.ResourceManager.GetString(resourceKey, culture);
    }

    internal static ValidationMessageResolver GetMessageResolver(ValidationBehaviorOptions? options = null)
        => new(options ?? new ValidationBehaviorOptions());
}
