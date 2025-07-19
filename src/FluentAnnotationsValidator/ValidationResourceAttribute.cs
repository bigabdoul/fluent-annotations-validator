namespace FluentAnnotationsValidator;

/// <summary>
/// 
/// </summary>
/// <param name="errorMessageResourceType"></param>

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ValidationResourceAttribute(Type errorMessageResourceType) : Attribute
{
    public Type ErrorMessageResourceType { get; } = errorMessageResourceType ?? throw new ArgumentNullException(nameof(errorMessageResourceType));
}
