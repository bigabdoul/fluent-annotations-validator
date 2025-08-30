namespace FluentAnnotationsValidator.Tests.Models;

// Test DTO used for unit tests.
public class BeforeValidationTestDto
{
    // A property to be validated and to demonstrate pre-validation value modification.
    public int Id { get; set; }

    // A string property that will be trimmed by the pre-validation delegate.
    public string? Name { get; set; }
}
