using FluentAnnotationsValidator.Metadata;

namespace FluentAnnotationsValidator.Tests.Models;

public class FluentRuleRegistrationDto
{
    [FluentRule(typeof(TestRegistrationDto))]
    public string Email { get; set; } = default!;
    
    [FluentRule(typeof(TestRegistrationDto))]
    public string Password { get; set; } = default!;

    [FluentRule(typeof(TestRegistrationDto))]
    public string? FirstName { get; set; }
    
    [FluentRule(typeof(TestRegistrationDto))]
    public string? LastName { get; set; }
    
    [FluentRule(typeof(TestRegistrationDto))]
    public DateTime? BirthDate { get; set; }
}

[InheritRules(typeof(TestRegistrationDto))]
public class InheritRulesRegistrationDto
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? BirthDate { get; set; }
}
