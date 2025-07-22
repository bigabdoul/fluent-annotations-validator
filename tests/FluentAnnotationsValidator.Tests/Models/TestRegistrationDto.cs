using System.ComponentModel.DataAnnotations;
using FluentAnnotationsValidator.Metadata;

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
}
