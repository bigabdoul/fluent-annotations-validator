using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FluentAnnotationsValidator;

public class PropertyValidationInfo
{
    public PropertyInfo Property { get; set; } = null!;
    public ValidationAttribute[] Attributes { get; set; } = Array.Empty<ValidationAttribute>();
}
