using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.AspNetCore;

public static class ServiceCollectionExtensions
{
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
