using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.AspNetCore.Tests.Models;

public record LoginDto(string Email, string Password, string? Role = null)
{
    [Required, EmailAddress]
    public string Email { get; init; } = Email;

    [Required]
    public string Password { get; init; } = Password;

    public string? Role { get; init; } = Role;
}

