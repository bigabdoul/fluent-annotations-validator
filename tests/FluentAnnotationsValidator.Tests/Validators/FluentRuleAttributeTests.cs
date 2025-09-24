using FluentAnnotationsValidator.Tests.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Validators;

public class FluentRuleAttributeTests
{
    private readonly ServiceCollection _services = new();
    private readonly ServiceProvider _serviceProvider;
    private readonly FluentTypeValidatorRoot _fluentTypeValidatorRoot;
    private readonly FluentTypeValidator<FluentRuleRegistrationDto> _configurator;

    private IFluentValidator<FluentRuleRegistrationDto> Validator =>
        _serviceProvider.GetRequiredService<IFluentValidator<FluentRuleRegistrationDto>>();

    public FluentRuleAttributeTests()
    {
        _services.AddFluentAnnotationsValidators(new ConfigurationOptions());
        _serviceProvider = _services.BuildServiceProvider();
        _fluentTypeValidatorRoot = _serviceProvider.GetRequiredService<FluentTypeValidatorRoot>();
        _configurator = _fluentTypeValidatorRoot.For<FluentRuleRegistrationDto>();
    }

    [Fact]
    public void FluentRule_Should_Validate_Members()
    {
        var dto = new FluentRuleRegistrationDto
        {
            Email = "User 1", // Invalid email: [EmailAddress]
            Password = "short", // Too short: [MinLength(6)]
            FirstName = $"{Guid.NewGuid()} {Guid.NewGuid()}", // Too long: [StringLength(50)]
        };

        // Act
        var result = Validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Email) && e.ErrorMessage.Contains("is not a valid e-mail address"));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Password) && e.ErrorMessage.Contains("minimum length"));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.FirstName) && e.ErrorMessage.Contains("maximum length"));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.LastName));
    }

    [Fact]
    public void FluentRule_Should_Validate_Members_WithNulls()
    {
        var dto = new FluentRuleRegistrationDto
        {
            Email = null!, // [Required]
            Password = "Strong!3290P@$$w0rD",
        };

        // Act
        var result = Validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Email) && e.AttemptedValue == null && e.ErrorMessage.Contains("required"));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Password));
    }


    [Fact]
    public void FluentRule_Should_Validate_Members_WithCustomRules()
    {
        var dto = new FluentRuleRegistrationDto
        {
            Email = null!, // [Required], Empty email: [NotEmpty]
            Password = "Strong!3290P@$$w0rD",
        };

        _configurator.RuleFor(d => d.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty.");

        _configurator.Build();

        // Act
        var result = Validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Email) && e.ErrorMessage.Contains("required"));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Email) && e.ErrorMessage.Contains("cannot be empty"));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(FluentRuleRegistrationDto.Password));
    }

    [Fact]
    public void InheritRules_Should_Validate_Static_Rules()
    {
        // Arrange

        // This model depends on the rules defined by TestRegistrationDto.
        var dto = new InheritRulesRegistrationDto
        {
            Email = "user@example.com",
            Password = "", // [Required], [MinLength(6)]
            LastName = $"{Guid.NewGuid()} {Guid.NewGuid()}", // Too long: [StringLength(50)]
        };

        var configurator = _fluentTypeValidatorRoot.For<InheritRulesRegistrationDto>();
        configurator.RuleFor(r => r.FirstName).NotEmpty().WithMessage("First name is required.");
        configurator.RuleFor(r => r.LastName).NotEmpty().WithMessage("Last name is required.");
        configurator.Build();

        var validator = _serviceProvider.GetRequiredService<IFluentValidator<InheritRulesRegistrationDto>>();

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.Password) && e.ErrorMessage.Contains("required"));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.Password) && e.ErrorMessage.Contains("minimum length"));
        
        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.FirstName) && e.ErrorMessage.Contains("required"));
        
        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.LastName) && e.ErrorMessage.Contains("maximum length"));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.LastName) && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void InheritRUles_Should_Validate_Static_And_Dynamic_Rules()
    {
        // Arrange
        var dto = new InheritRulesRegistrationDto
        {
            Email = "user", // Invalid: [EmailAddress]
            Password = "Strong!3290P@$$w0rD",
            FirstName = "Jean", // Invalid: Must be "Jonathan"
            LastName = "Dupont", // Invalid: Must be "Doe"
        };

        // Further configure TestRegistrationDto by adding dynamic rules, which
        // should get picked up by the validator of InheritRulesRegistrationDto.
        var dynamicConfig = _fluentTypeValidatorRoot.For<TestRegistrationDto>();

        dynamicConfig.RuleFor(x => x.FirstName)
            .Must(firstname => firstname == "Jonathan")
            .WithMessage("The first name must be 'Jonathan'.");
        
        dynamicConfig.RuleFor(x => x.LastName)
            .Must(lastname => lastname == "Doe")
            .WithMessage("The last name must be 'Doe'.");

        dynamicConfig.Build();

        var configurator = _fluentTypeValidatorRoot.For<InheritRulesRegistrationDto>();
        configurator.RuleFor(r => r.FirstName)
            .ExactLength(8)
            .WithMessage("{0} must be exactly {1} chars long.");

        configurator.Build();

        var validator = _serviceProvider.GetRequiredService<IFluentValidator<InheritRulesRegistrationDto>>();

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.Email) && e.ErrorMessage.Contains("not a valid e-mail"));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.Password));

        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.FirstName) && e.ErrorMessage.Contains("must be 'Jonathan'"));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(InheritRulesRegistrationDto.LastName) && e.ErrorMessage.Contains("must be 'Doe'"));
    }
}
