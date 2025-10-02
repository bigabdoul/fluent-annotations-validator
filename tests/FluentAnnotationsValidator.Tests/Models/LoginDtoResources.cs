namespace FluentAnnotationsValidator.Tests.Models;

using Resources;

[ValidationResource(typeof(ConventionValidationMessages))]
public class TestLoginDtoWithResource
{
    public string? Email { get; set; }
}

[ValidationResource(typeof(ConventionValidationMessages))]
public class LoginDtoWithResource
{
    public string? Email { get; set; }
}
