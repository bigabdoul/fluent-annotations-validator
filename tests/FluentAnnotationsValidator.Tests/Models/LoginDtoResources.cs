using FluentAnnotationsValidator.Metadata;
using FluentAnnotationsValidator.Tests.Resources;

namespace FluentAnnotationsValidator.Tests.Models;

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
