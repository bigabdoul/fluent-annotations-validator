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
    /// Registers <see cref="IFluentValidator"/> services for all discovered types that contain
    /// at least one property decorated with a <see cref="ValidationAttribute"/>.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>A <see cref="FluentAnnotationsBuilder"/> for further configuration chaining.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services, params Type[] targetAssembliesTypes) =>
        services.AddFluentAnnotationsValidators(configure: null, extraValidatableTypes: null, targetAssembliesTypes);

    /// <summary>
    /// Registers <see cref="IFluentValidator"/> services for all discovered types that contain
    /// at least one property decorated with a <see cref="ValidationAttribute"/>.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="extraValidatableTypes">A function to include additional types into the validation pipeline.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>A <see cref="FluentAnnotationsBuilder"/> for further configuration chaining.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services, Func<IEnumerable<Type>>? extraValidatableTypes, params Type[] targetAssembliesTypes) =>
        services.AddFluentAnnotationsValidators(configure: null, extraValidatableTypes, targetAssembliesTypes);

    /// <summary>
    /// Forwards the call to <see cref="AddFluentAnnotationsValidators(IServiceCollection, Action{ValidationBehaviorOptions}?, Action{LocalizationOptions}?, Func{IStringLocalizerFactory, StringLocalizerFactoryResult}?, Func{IEnumerable{Type}}?, Type[])"/>.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="configure">An action to configure a <see cref="ValidationBehaviorOptions"/>.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>A <see cref="FluentAnnotationsBuilder"/> for further configuration chaining.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services,
    Action<ValidationBehaviorOptions>? configure = null,
    params Type[] targetAssembliesTypes) =>
        services.AddFluentAnnotationsValidators(configure, extraValidatableTypes: null, targetAssembliesTypes);

    /// <summary>
    /// Forwards the call to <see cref="AddFluentAnnotationsValidators(IServiceCollection, Action{ValidationBehaviorOptions}?, Action{LocalizationOptions}?, Func{IStringLocalizerFactory, StringLocalizerFactoryResult}?, Func{IEnumerable{Type}}?, Type[])"/>.
    /// </summary>
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="configure">An action to configure a <see cref="ValidationBehaviorOptions"/>.</param>
    /// <param name="extraValidatableTypes">A function to include additional types into the validation pipeline.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>A <see cref="FluentAnnotationsBuilder"/> for further configuration chaining.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services,
    Action<ValidationBehaviorOptions>? configure = null,
    Func<IEnumerable<Type>>? extraValidatableTypes = null,
    params Type[] targetAssembliesTypes)
    {
        return services.AddFluentAnnotationsValidators(configure,
            configureLocalization: null,
            localizerFactory: null,
            extraValidatableTypes,
            targetAssembliesTypes);
    }

    /// <summary>
    /// Registers <see cref="IFluentValidator"/> services for all discovered types that contain
    /// at least one property or field decorated with a <see cref="ValidationAttribute"/>, or
    /// any type that implements the marker interface <see cref="IFluentValidatable"/>.
    /// 
    /// Scans the provided assemblies (via types) or defaults to the current AppDomain.
    /// This method dynamically generates and registers transient validators based on
    /// <see cref="FluentValidator{T}"/> for all matching types.
    /// 
    /// <para>
    /// <strong>Scanning Rationale:</strong>
    /// This method intentionally inspects property-level annotations rather than class-level
    /// attributes. Most validation attributes (e.g., <c>[Required]</c>, <c>[EmailAddress]</c>, 
    /// <c>[StringLength]</c>) are designed to be applied directly to properties � not to types.
    /// </para>
    /// 
    /// <para>
    /// <strong>Class-level limitation:</strong>
    /// While <see cref="ValidationAttribute"/> is technically applicable to classes, the built-in
    /// attributes do not support or implement meaningful class-level validation semantics.
    /// Supporting class-level discovery may lead to false positives or require custom attribute usage.
    /// Instead, implement <see cref="IFluentValidatable"/> to include a type into the validation
    /// pipeline, or use the <paramref name="extraValidatableTypes"/> to specify the extra types to include.
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
    /// <param name="extraValidatableTypes">A function to include additional types into the validation pipeline.</param>
    /// <param name="targetAssembliesTypes">
    /// Optional: One or more types used to infer target assemblies to scan.
    /// If omitted, all assemblies in the current AppDomain are scanned.
    /// </param>
    /// <returns>A <see cref="FluentAnnotationsBuilder"/> for further configuration chaining.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services,
    Action<ValidationBehaviorOptions>? configure = null,
    Action<LocalizationOptions>? configureLocalization = null,
    Func<IStringLocalizerFactory, StringLocalizerFactoryResult>? localizerFactory = null,
    Func<IEnumerable<Type>>? extraValidatableTypes = null,
    params Type[] targetAssembliesTypes) => services.AddFluentAnnotationsValidators(new ConfigurationOptions
    {
        ConfigureBehaviorOptions = configure,
        ConfigureLocalization = configureLocalization,
        ExtraValidatableTypes = extraValidatableTypes,
        LocalizerFactory = localizerFactory,
        TargetAssembliesTypes = targetAssembliesTypes,
    });

    /// <summary>
    /// Registers and configures the Fluent Annotations Validator in the service collection.
    /// </summary>
    /// <remarks>
    /// This extension method simplifies the setup process by using a single
    /// <see cref="ConfigurationOptions"/> object to manage all configurations,
    /// including behavior, localization, and assembly scanning.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configurationOptions">An object containing all the configuration options for the validator setup.</param>
    /// <returns>A <see cref="FluentAnnotationsBuilder"/> for further configuration chaining.</returns>
    public static FluentAnnotationsBuilder AddFluentAnnotationsValidators(this IServiceCollection services, ConfigurationOptions configurationOptions)
    {
        var targetAssembliesTypes = configurationOptions.TargetAssembliesTypes;
        var assemblies = targetAssembliesTypes.Length > 0
            ? [.. targetAssembliesTypes.Select(t => t.Assembly)]
            : AppDomain.CurrentDomain.GetAssemblies();

        // Scan all model types in application assembly
        var attributeDecoratedTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t =>
                typeof(IFluentValidatable).IsAssignableFrom(t) ||
                t.GetProperties().Any(p => p.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0) ||
                t.GetFields().Any(p => p.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0) ||

                // records may have attribute-decorated members in the constructor:
                // public record LoginDto([Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
                t.GetConstructors().Any(p => p.GetCustomAttributes(typeof(ValidationAttribute), true).Length > 0)
            ).ToList();

        var extraValidatableTypes = configurationOptions.ExtraValidatableTypes;

        if (extraValidatableTypes != null)
        {
            // Get any additional types from the provided function and combine them
            var additionalTypes = extraValidatableTypes.Invoke();
            attributeDecoratedTypes = additionalTypes.Any() ? [.. attributeDecoratedTypes.Union(additionalTypes)] : attributeDecoratedTypes;
        }

        var behaviorOptions = new ValidationBehaviorOptions();
        configurationOptions.ConfigureBehaviorOptions?.Invoke(behaviorOptions);

        var builder = new FluentAnnotationsBuilder(services, behaviorOptions);

        // Filter the types to only include the most derived classes
        var mostDerivedTypes = attributeDecoratedTypes
            .Where(type => !attributeDecoratedTypes.Any(otherType => otherType.IsSubclassOf(type)))
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
        services.AddLocalization(options => configurationOptions.ConfigureLocalization?.Invoke(options));

        if (configurationOptions.LocalizerFactory != null)
        {
            var tempProvider = services.BuildServiceProvider();
            var factory = tempProvider.GetRequiredService<IStringLocalizerFactory>();
            var result = configurationOptions.LocalizerFactory.Invoke(factory);

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
    /// <param name="services">The DI container to register validators into.</param>
    /// <param name="configure">An action to configure a <see cref="ValidationConfigurator"/>. Can be null.</param>
    /// <param name="configureBehavior">An action to configure a <see cref="ValidationBehaviorOptions"/>. Can be null.</param>
    /// <param name="configureLocalization">An action delegate to configure <see cref="LocalizationOptions"/>. Can be null.</param>
    /// <param name="localizerFactory">An action delegate to configure <see cref="IStringLocalizerFactory"/>. Can be null.</param>
    /// <param name="extraValidatableTypes">A function to include additional types into the validation pipeline.</param>
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
    Func<IEnumerable<Type>>? extraValidatableTypes = null,
    params Type[] targetAssembliesTypes) => services.AddFluentAnnotations(new ConfigurationOptions
    {
        ConfigureBehaviorOptions = configureBehavior,
        ConfigureValidationConfigurator = configure,
        ConfigureLocalization = configureLocalization,
        ExtraValidatableTypes = extraValidatableTypes,
        LocalizerFactory = localizerFactory,
        TargetAssembliesTypes = targetAssembliesTypes,
    });

    /// <summary>
    /// Registers and configures the Fluent Annotations Validator in the service collection.
    /// </summary>
    /// <remarks>
    /// This extension method simplifies the setup process by using a single
    /// <see cref="ConfigurationOptions"/> object to encapsulate all configurations,
    /// including validation behavior, localization, and assembly scanning. The method
    /// returns the <see cref="IServiceCollection"/> to enable further method chaining.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the validator services to.</param>
    /// <param name="configurationOptions">An object containing all the configuration options for the validator setup.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional services can be chained.</returns>
    public static IServiceCollection AddFluentAnnotations(this IServiceCollection services, ConfigurationOptions configurationOptions)
    {
        var builder = services.AddFluentAnnotationsValidators(configurationOptions);
        var configurator = builder.UseFluentAnnotations();
        configurationOptions.ConfigureValidationConfigurator?.Invoke(configurator);
        return services;
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
}
