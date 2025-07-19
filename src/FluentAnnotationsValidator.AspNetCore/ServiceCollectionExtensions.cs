using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.AspNetCore;

/// <summary>
/// ASP.NET Core-specific service registration utilities for FluentAnnotationsValidator.
/// Automatically discovers and registers <see cref="IValidator{T}"/> instances for types decorated
/// with <see cref="ValidationAttribute"/>s.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Scans all loaded assemblies for classes decorated with <see cref="ValidationAttribute"/>s and
    /// registers corresponding <see cref="DataAnnotationsValidator{T}"/> validators via DI.
    /// Supports both Minimal APIs and MVC-style validation workflows.
    /// </summary>
    /// <param name="services">The service collection into which validators should be registered.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFluentAnnotationsValidators(this IServiceCollection services)
    {
        var validatorType = typeof(IValidator<>);
        var assembly = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in assembly)
        {
            foreach (var type in asm.GetTypes().Where(t => t.IsClass && t.GetCustomAttributes<ValidationAttribute>().Any()))
            {
                var validatorImpl = typeof(DataAnnotationsValidator<>).MakeGenericType(type);
                var validatorInterface = typeof(IValidator<>).MakeGenericType(type);
                services.AddTransient(validatorInterface, validatorImpl);
            }
        }

        return services;
    }
}
