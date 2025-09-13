namespace FluentAnnotationsValidator.Tests.Models;

// A simple DTO for testing conditional validation.
public class ConditionalTestDto
{
    public int Age { get; set; }
    public string? Name { get; set; }
    public ICollection<ConditionalTestItemDto> Items { get; set; } = [];
}

public class ConditionalTestItemDto
{
    public string ItemName { get; set; } = default!;

    public ICollection<TestProductModel> Products { get; set; } = [];
}