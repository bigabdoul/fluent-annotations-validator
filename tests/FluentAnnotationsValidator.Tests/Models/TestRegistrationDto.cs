using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Tests.Models;

using Resources;

[ValidationResource(typeof(ValidationMessages))]
public class TestRegistrationDto
{
    [Required(ErrorMessageResourceName = nameof(ValidationMessages.EmailAddressRequired))]
    [EmailAddress(ErrorMessageResourceName = nameof(ValidationMessages.EmailAddressInvalid))]
    public string Email { get; set; } = default!;

    [Required(ErrorMessageResourceName = nameof(ValidationMessages.PasswordRequired))]
    [MinLength(6, ErrorMessageResourceName = nameof(ValidationMessages.PasswordMinLength))]
    public string Password { get; set; } = default!;
}