using FluentAnnotationsValidator.AspNetCore.Tests.Models;

namespace FluentAnnotationsValidator.AspNetCore.Tests.Validators;

public class LoginDtoValidator(IValidationMessageResolver resolver) 
: DataAnnotationsValidator<LoginDto>(resolver)
{
}