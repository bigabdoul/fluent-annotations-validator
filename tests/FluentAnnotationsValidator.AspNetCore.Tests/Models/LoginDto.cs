using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.AspNetCore.Tests.Models;

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required] public string Password {  get; set; } = default!;
}
