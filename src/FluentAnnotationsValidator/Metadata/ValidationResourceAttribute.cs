namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies a default <see cref="ErrorMessageResourceType"/> to be used for resolving localized error messages
/// for all <see cref="System.ComponentModel.DataAnnotations.ValidationAttribute"/>s applied to a model class.
/// This centralizes resource configuration, avoiding repetition across individual attributes.
/// </summary>
/// <param name="errorMessageResourceType">The type containing localized error message properties.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ValidationResourceAttribute(Type errorMessageResourceType) : Attribute
{
    /// <summary>
    /// Gets the type that provides static string properties for error message resources.
    /// This type should contain properties named according to convention or explicitly referenced by validation attributes.
    /// </summary>
    public Type ErrorMessageResourceType { get; } = errorMessageResourceType ?? throw new ArgumentNullException(nameof(errorMessageResourceType));
}
