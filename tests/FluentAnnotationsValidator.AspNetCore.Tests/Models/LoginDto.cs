using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.AspNetCore.Tests.Models;

public record LoginDto
(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? Role = null
);
