using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Runtime.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator.Extensions;

/// <summary>
/// ASP.NET Core-specific service registration utilities for FluentAnnotationsValidator.
/// Automatically discovers and registers <see cref="IFluentValidator{T}"/> instances for types decorated
/// with <see cref="ValidationAttribute"/>s.
/// </summary>
public static class ValidatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <c>IValidator&lt;T&gt;</c> services for all discovered types that contain
    /// at least one property decorated with a <see cref="ValidationAttribute"/>.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services, params Type[] targetAssembliesTypes) =>
        services.AddFluentAnnotationsValidators(configure: null, targetAssembliesTypes);

    /// <summary>
    /// Forwards the call to <see cref="AddFluentAnnotationsValidators(IServiceCollection, Action{ValidationBehaviorOptions}?, Action{LocalizationOptions}?, Func{IStringLocalizerFactory, StringLocalizerFactoryResult}?, Type[])"/>.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="configure">An action to configure a <see cref="ValidationBehaviorOptions"/>.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services,
    Action<ValidationBehaviorOptions>? configure = null, params Type[] targetAssembliesTypes) =>
        services.AddFluentAnnotationsValidators(configure, 
            configureLocalization: null, 
            localizerFactory: null, 
            targetAssembliesTypes);

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
    /// <param name="configure">An action to configure a <see cref="ValidationBehaviorOptions"/>.</param>
    /// <param name="configureLocalization">A delegate to invoke to further configure <see cref="LocalizationOptions"/>. Can be null.</param>
    /// <param name="localizerFactory">An action delegate to invoke to further configure <see cref="IStringLocalizerFactory"/>. Can be null.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services,
    Action<ValidationBehaviorOptions>? configure = null, 
    Action<LocalizationOptions>? configureLocalization = null,
    Func<IStringLocalizerFactory, StringLocalizerFactoryResult>? localizerFactory = null,
    params Type[] targetAssembliesTypes)
    {
        var assemblies = targetAssembliesTypes.Length > 0
            ? [.. targetAssembliesTypes.Select(t => t.Assembly)]
            : AppDomain.CurrentDomain.GetAssemblies();

        // Scan all model types in application assembly
        var modelTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t =>
                t.GetProperties().Any(p => p.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0) ||
                t.GetFields().Any(p => p.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0) ||

                // records may have attribute-decorated members in the constructor:
                // public record LoginDto([Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
                t.GetConstructors().Any(p => p.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0)
            ).ToList();

        var behaviorOptions = new ValidationBehaviorOptions();
        configure?.Invoke(behaviorOptions);

        var builder = new FluentAnnotationsBuilder(services, behaviorOptions);

        // Filter the types to only include the most derived classes
        var mostDerivedTypes = modelTypes
            .Where(type => !modelTypes.Any(otherType => otherType.IsSubclassOf(type)))
            .ToList();

        foreach (var modelType in mostDerivedTypes)
        {
            var genericValidator = typeof(IFluentValidator<>).MakeGenericType(modelType);

            if (services.Any(sd => sd.ServiceType == genericValidator))
                continue; // Skip redundant registration

            // Dynamically create validator for each type
            var validatorType = typeof(FluentValidator<>).MakeGenericType(modelType);
            services.AddScoped(genericValidator, validatorType);

            // Register validation rules upfront (optional)
            var members = modelType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m is PropertyInfo or FieldInfo /*or ConstructorInfo or MethodInfo*/);

            foreach (var member in members)
            {
                var rules = ValidationAttributeAdapter.ParseRules(modelType, member);
                behaviorOptions.AddRules(member, rules);
            }
        }

        services.AddTransient(_ => behaviorOptions);

        services.AddLogging(); // Localizaer factory needs logging support

        // Register and configure Localization services...
        services.AddLocalization(options => configureLocalization?.Invoke(options));

        if (localizerFactory != null)
        {
            var tempProvider = services.BuildServiceProvider();
            var factory = tempProvider.GetRequiredService<IStringLocalizerFactory>();
            var result = localizerFactory.Invoke(factory);

            if (result?.SharedResourceType != null)
            {
                factory.Create(result.SharedResourceType);
                behaviorOptions.SharedResourceType ??= result.SharedResourceType;
            }

            if (result?.SharedCulture != null)
            {
                behaviorOptions.SharedCulture ??= result.SharedCulture;
            }
        }

        //services.AddDataAnnotationsLocalization();

        // Register the custom message resolver and the main validator service
        services.AddSingleton<IValidationMessageResolver, ValidationMessageResolver>();

        return builder;
    }

    /// <summary>
    /// Registers and initializes FluentAnnotations services and validation configuration into the dependency injection container.
    /// </summary>
    /// <param name="builder">A fluent configuration builder used instantiate a <see cref="ValidationConfigurator"/>.</param>
    /// <returns>
    /// A <see cref="ValidationConfigurator"/> instance that allows for fluent configuration of conditional validation rules.
    /// </returns>
    public static ValidationConfigurator UseFluentAnnotations(this FluentAnnotationsBuilder builder)
    {
        var configurator = new ValidationConfigurator(builder.Options);
        return configurator;
    }

    /// <summary>
    /// Registers and initializes FluentAnnotations services and validation configuration into the dependency injection container.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="configure">An action to configure a <see cref="ValidationConfigurator"/>. Can be null.</param>
    /// <param name="configureBehavior">An action to configure a <see cref="ValidationBehaviorOptions"/>. Can be null.</param>
    /// <param name="configureLocalization">An action delegate to configure <see cref="LocalizationOptions"/>. Can be null.</param>
    /// <param name="localizerFactory">An action delegate to configure <see cref="IStringLocalizerFactory"/>. Can be null.</param>
    /// <param name="targetAssembliesTypes">
    /// One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddFluentAnnotations(this IServiceCollection services,
    Action<ValidationConfigurator>? configure = null,
    Action<ValidationBehaviorOptions>? configureBehavior = null,
    Action<LocalizationOptions>? configureLocalization = null,
    Func<IStringLocalizerFactory, StringLocalizerFactoryResult>? localizerFactory = null,
    params Type[] targetAssembliesTypes)
    {
        var builder = services.AddFluentAnnotationsValidators(configureBehavior,
            configureLocalization,
            localizerFactory,
            targetAssembliesTypes);

        var configurator = builder.UseFluentAnnotations();

        configure?.Invoke(configurator);
        return services;
    }
}
