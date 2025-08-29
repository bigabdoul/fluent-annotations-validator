using FluentAnnotationsValidator.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Models;

public class ValidationTypeConfiguratorTestModel
{
    [Required, MinLength(5)]
    public string? Name { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    public string Password { get; set; } = default!;

    [Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
    public string? ConfirmEmail { get; set; }
    public int Age { get; set; }
}

public class TestProductModel : IFluentValidatable
{
    public bool IsPhysicalProduct { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
}
