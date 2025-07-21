using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Models;

using FluentAnnotationsValidator.Metadata;
using Resources;

[ValidationResource(typeof(ValidationMessages))]
public class TestRegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;
}