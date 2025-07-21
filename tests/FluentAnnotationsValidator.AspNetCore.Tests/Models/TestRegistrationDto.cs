using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.AspNetCore.Tests.Models;

public class TestRegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;
}