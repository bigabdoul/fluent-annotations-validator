using FluentAnnotationsValidator.Metadata;

namespace FluentAnnotationsValidator.Tests.Messages;

[ValidationResource(typeof(ConventionValidationMessages))]
public class LoginDtoWithResource
{
    public string? Email { get; set; }
}
