using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.AspNetCore.Tests.Models;

// Don't use validation attributes on positional parameters in a record like this:
// public record LoginDto([Required, EmailAddress] string Email, [Required] string Password, string? Role = null);
// Apply them directly to the properties as shown below.
public record TestLoginDto(string Email, string Password, string? Role = null)
{
    [Required, EmailAddress]
    public string Email { get; } = Email;

    [Required]
    public string Password { get; } = Password;

    public string? Role { get; } = Role;
}

