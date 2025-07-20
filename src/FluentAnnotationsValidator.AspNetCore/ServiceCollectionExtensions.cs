using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    /// Registers <c>IValidator&lt;T&gt;</c> services for all discovered types that contain
    /// at least one property decorated with a <see cref="ValidationAttribute"/>.
    /// 
    /// Scans the provided assemblies (via types) or defaults to the current AppDomain.
    /// This method dynamically generates and registers transient validators based on
    /// <c>DataAnnotationsValidator&lt;T&gt;</c> for all matching types.
    /// 
    /// <para>
    /// <strong>Scanning Rationale:</strong>
    /// This method intentionally inspects property-level annotations rather than class-level
    /// attributes. Most validation attributes (e.g., <c>[Required]</c>, <c>[EmailAddress]</c>, 
    /// <c>[StringLength]</c>) are designed to be applied directly to properties — not to types.
    /// </para>
    /// 
    /// <para>
    /// <strong>Class-level limitation:</strong>
    /// While <see cref="ValidationAttribute"/> is technically applicable to classes, the built-in
    /// attributes do not support or implement meaningful class-level validation semantics.
    /// Supporting class-level discovery may lead to false positives or require custom attribute usage.
    /// </para>
    /// 
    /// <para>
    /// <strong>Extensibility:</strong>
    /// If consumers author custom validation attributes targeting types, they can opt into
    /// alternative scanning via a custom DI extension method or marker interfaces.
    /// </para>
    /// 
    /// <para>
    /// <strong>Compatibility:</strong>
    /// This method also registers a singleton <see cref="IValidationMessageResolver"/> unless one
    /// has been previously added, enabling localized message resolution.
    /// </para>
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddFluentAnnotationsValidators(this IServiceCollection services, params Type[] targetAssembliesTypes)
    {
        var validatorType = typeof(IValidator<>);
        var assemblies = targetAssembliesTypes.Length > 0
            ? [.. targetAssembliesTypes.Select(t => t.Assembly)]
            : AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in assemblies)
        {
            // retrieve all classes having at least one property
            // decorated with ValidationAttribute custom attribute
            IList<Type> propertiesWithValidationAttributes = [..asm.GetTypes().Where(t =>
                t.IsClass &&
                t.GetProperties().Any(p => p.GetCustomAttributes<ValidationAttribute>().Any())
            )];

            foreach (var type in propertiesWithValidationAttributes)
            {
                var validatorImpl = typeof(DataAnnotationsValidator<>).MakeGenericType(type);
                var validatorInterface = validatorType.MakeGenericType(type);
                services.AddTransient(validatorInterface, validatorImpl);
            }
        }

        // Try to add a default validation message resolver.
        // This has no effect if a custom resolver has been previously added.
        services.TryAddSingleton<IValidationMessageResolver, ValidationMessageResolver>();

        return services;
    }
}
