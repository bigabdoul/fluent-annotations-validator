using FluentAnnotationsValidator.Tests.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Validators;

public class FluentRuleAttributeTests
{
    private readonly ServiceCollection _services = new();
    private FluentTypeValidator<FluentRuleRegistrationDto> _configurator;

    private IFluentValidator<FluentRuleRegistrationDto> Validator =>
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<FluentRuleRegistrationDto>>();

    public FluentRuleAttributeTests()
    {
        _services.AddFluentAnnotations(new ConfigurationOptions
        {
            ConfigureValidatorRoot = config => _configurator = config.For<FluentRuleRegistrationDto>(),
            //ExtraValidatableTypesFactory = () => [typeof(ConditionalTestDto)],
        });

        ArgumentNullException.ThrowIfNull(_configurator);
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
}
