using FluentAnnotationsValidator.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace FluentAnnotationsValidator.Configuration;
using Extensions;
using FluentAnnotationsValidator.Abstractions;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides a single, unified container for configuring the Fluent Annotations Validator.
/// </summary>
/// <remarks>
/// This class encapsulates all the optional parameters for the
/// <see cref="ValidatorServiceCollectionExtensions.AddFluentAnnotationsValidators(IServiceCollection, ConfigurationOptions)"/>
/// method, improving API clarity and maintainability by allowing all configurations to be passed as a single object.
/// </remarks>
public class ConfigurationOptions
{
    /// <summary>
    /// Gets or sets an action to configure the core validation behavior, such as rule registration.
    /// </summary>
    /// <remarks>
    /// Use this delegate to customize how rules are registered and validated.
    /// </remarks>
    public Action<ValidationBehaviorOptions>? ConfigureBehaviorOptions { get; set; }

    /// <summary>
    /// Gets or sets the delegate used to configure the core validation rules for the application.
    /// </summary>
    /// <remarks>
    /// This action provides the main entry point for users to define and customize
    /// their validation rules via the <see cref="ValidationConfigurator"/>.
    /// </remarks>
    public Action<ValidationConfigurator>? ConfigureValidationConfigurator { get; set; }

    /// <summary>
    /// Gets or sets an action to configure the localization settings for validation messages.
    /// </summary>
    /// <remarks>
    /// This delegate allows for fine-tuning how validation messages are localized.
    /// </remarks>
    public Action<LocalizationOptions>? ConfigureLocalization { get; set; }

    /// <summary>
    /// Gets or sets a factory function for creating the string localizer.
    /// </summary>
    /// <remarks>
    /// This function provides a hook for advanced localization scenarios 
    /// where a custom <see cref="IStringLocalizerFactory"/> is required.
    /// </remarks>
    public Func<IStringLocalizerFactory, StringLocalizerFactoryResult>? LocalizerFactory { get; set; }

    /// <summary>
    /// Gets or sets a function that provides additional types to be validated.
    /// </summary>
    /// <remarks>
    /// This function is useful for including types from sources not available 
    /// at compile time, such as dynamically loaded assemblies, or for adding 
    /// validation to types that do not have any <see cref="ValidationAttribute"/>s,
    /// and don't implement the <see cref="IFluentValidatable"/> marker interface.
    /// </remarks>
    public Func<IEnumerable<Type>>? ExtraValidatableTypesFactory { get; set; }

    /// <summary>
    /// Gets or sets an array of types from the target assemblies to scan for validatable DTOs.
    /// </summary>
    /// <remarks>
    /// The validator will scan the assemblies containing these types to discover
    /// and register validation rules based on attributes and other configurations.
    /// </remarks>
    public Type[]? TargetAssembliesTypes { get; set; }
}

