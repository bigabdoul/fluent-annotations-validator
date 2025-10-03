using System.ComponentModel.DataAnnotations;

namespace Demo.Application.DTOs;

public class IdentityModelBase
{
    [Required, EmailAddress]
    public virtual string Email { get; set; } = default!;

    [Required, MinLength(8), DataType(DataType.Password)]
    public virtual string Password { get; set; } = default!;
}
