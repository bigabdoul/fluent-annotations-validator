using FluentAnnotationsValidator.Metadata;

namespace FluentAnnotationsValidator.Tests.Models;

//[FluentRuleFrom(typeof(TestRegistrationDto))]
public class FluentRuleRegistrationDto : IFluentValidatable
{
    [FluentRule(typeof(TestRegistrationDto))]
    public string Email { get; set; } = default!;
    
    [FluentRule(typeof(TestRegistrationDto))]
    public string Password { get; set; } = default!;

    [FluentRule(typeof(TestRegistrationDto))]
    public string? FirstName { get; set; }
    
    [FluentRule(typeof(TestRegistrationDto))]
    public string? LastName { get; set; }
    
    public DateTime? BirthDate { get; set; }
}
