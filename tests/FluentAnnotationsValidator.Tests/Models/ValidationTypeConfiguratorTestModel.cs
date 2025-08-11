using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Models;

public class ValidationTypeConfiguratorTestModel
{
    [Required, MinLength(5)]
    public string? Name { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public int Age { get; set; }
    public bool IsPhysicalProduct { get; internal set; }
    public string ShippingAddress { get; internal set; } = string.Empty;
}
