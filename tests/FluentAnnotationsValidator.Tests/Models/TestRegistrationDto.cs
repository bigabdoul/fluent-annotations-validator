using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Models;

[ValidationResource(typeof(Resources.ValidationMessages))]
public class TestRegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }
}
