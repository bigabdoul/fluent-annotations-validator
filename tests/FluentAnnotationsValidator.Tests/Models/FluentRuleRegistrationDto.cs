using FluentAnnotationsValidator.Annotations;

namespace FluentAnnotationsValidator.Tests.Models;

public class FluentRuleRegistrationBase
{
    public virtual string Email { get; set; } = default!;

    public virtual string Password { get; set; } = default!;

    public virtual string? FirstName { get; set; }

    public virtual string? LastName { get; set; }

    public virtual DateTime? BirthDate { get; set; }
}

public class FluentRuleRegistrationDto : FluentRuleRegistrationBase
{
    [FluentRule(typeof(TestRegistrationDto))]
    public override string Email { get; set; } = default!;
    
    [FluentRule(typeof(TestRegistrationDto))]
    public override string Password { get; set; } = default!;

    [FluentRule(typeof(TestRegistrationDto))]
    public override string? FirstName { get; set; }
    
    [FluentRule(typeof(TestRegistrationDto))]
    public override string? LastName { get; set; }
    
    [FluentRule(typeof(TestRegistrationDto))]
    public override DateTime? BirthDate { get; set; }
}

[InheritRules(typeof(TestRegistrationDto))]
public class InheritRulesRegistrationDto : FluentRuleRegistrationBase
{
}

[InheritRulesAsync(typeof(TestRegistrationDto))]
public class InheritRulesAsyncRegistrationDto : FluentRuleRegistrationBase
{
}
