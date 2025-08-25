using FluentAnnotationsValidator.Metadata;

namespace FluentAnnotationsValidator.Tests.Messages;

public static class ConventionValidationMessages
{
    public static string Email_Required => "Email is required (convention).";
    public const string Password_MustValidation = "Password is not complex enough.";
}

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
