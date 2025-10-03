using FluentAnnotationsValidator.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Demo.Application.DTOs;

public class RegisterModel : IdentityModelBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [NotEmpty, MaxLength(50)]
    public string FirstName { get; set; } = default!;

    [NotEmpty, MaxLength(50)]
    public string LastName { get; set; } = default!;

    [NotEmpty, MaxLength(20)]
    public string PhoneNumber { get; set; } = default!;
}