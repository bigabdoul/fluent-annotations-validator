using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Validators;
using static TestHelpers;

public partial class RegistrationModelTests
{
    private readonly ServiceProvider _serviceProvider;

    public RegistrationModelTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddFluentAnnotations(configure: config =>
            {
                // Rule for password complexity
                var configurator = config.For<TestRegistrationDto>();

                // Non-preemptive rule (preserves all previously registered rules,
                // including those hard-coded custom attributes applied to the
                // TestRegistrationDto.Password property).
                configurator.RuleFor(x => x.Password)
                    .Must(BeComplexPassword)
                    .WithMessage(ConventionValidationMessages.Password_MustValidation);

                configurator.Build();

            }, targetAssembliesTypes: typeof(TestRegistrationDto))
            .AddTransient(provider =>
            {
                var options = provider.GetRequiredService<ValidationBehaviorOptions>();
                return new ValidationConfigurator(options);
            })
            .AddTransient<ITestRegistrationConfigurator, TestRegistrationConfigurator>()
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData("Abc1234!", true)]
    [InlineData("password", false)]
    [InlineData("PASSWORD", false)]
    [InlineData("12345678", false)]
    [InlineData("Abcdefgh", false)]
    [InlineData("Abc12", false)] // too short
    public void NonPreemptive_RuleFor_ComplexPassword_Should_Return_CorrectResult(string password, bool expectedResult)
    {
        // Arrange
        var model = new TestRegistrationDto { Password = password, Email = "test@example.com" };
        var validator = _serviceProvider.GetRequiredService<IFluentValidator<TestRegistrationDto>>();

        // Act
        var result = validator.Validate(model);

        // Assert
        if (expectedResult)
        {
            result.IsValid.Should().BeTrue();
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(TestRegistrationDto.Password) &&
                e.ErrorMessage == ConventionValidationMessages.Password_MustValidation);
        }
    }

    [Fact]
    public void Except_ShouldRemoveStaticAttributeRulesForProperty()
    {
        // Arrange
        var configurator = _serviceProvider.GetRequiredService<ITestRegistrationConfigurator>();

        // The TestRegistrationDto.Email property has a [Required] attribute.
        // We ensure it's registered by building the default configuration first.
        _serviceProvider.GetRequiredService<ValidationConfigurator>().For<TestRegistrationDto>().Build();

        // Act
        // Now, clear all current rules from the configurator for a clean state
        //configurator.ClearRules();
        configurator.RemoveRulesExceptFor(x => x.Email);

        // Exclude the Email property
        configurator.Except(x => x.Email);
        configurator.Build(); // Rebuild to apply the 'Except' rule

        // Assert
        var validator = _serviceProvider.GetRequiredService<IFluentValidator<TestRegistrationDto>>();
        var model = new TestRegistrationDto { Email = null!, Password = "validPassword1!", FirstName = "Test" };
        var result = validator.Validate(model, throwWhenNoRules: false);

        // The Email field should no longer produce a "Required" error
        result.IsValid.Should().BeTrue("Email should be excluded from validation and not cause a 'Required' error.");
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(TestRegistrationDto.Email) && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Except_ShouldRemoveDynamicallyAddedRulesForProperty()
    {
        // Arrange
        var configurator = _serviceProvider.GetRequiredService<ITestRegistrationConfigurator>();

        // Clear existing rules to isolate this test's scenario
        configurator.ClearRules();

        // Dynamically add a rule for FirstName
        configurator.RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name cannot be empty.");

        // Act
        // Exclude the FirstName property that now has a dynamic rule
        configurator.Except(x => x.FirstName);
        configurator.Build();

        // Assert
        var validator = _serviceProvider.GetRequiredService<IFluentValidator<TestRegistrationDto>>();
        var model = new TestRegistrationDto { FirstName = "", Email = "test@example.com", Password = "validPassword1!" };
        var result = validator.Validate(model, throwWhenNoRules: false);

        // FirstName should now be excluded from validation, so no 'NotEmpty' error
        result.IsValid.Should().BeTrue("FirstName should be excluded from validation and not cause a 'NotEmpty' error.");
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(TestRegistrationDto.FirstName) && e.ErrorMessage.Contains("empty"));
    }

    [Fact]
    public void Except_ShouldNotAffectRulesOnOtherProperties()
    {
        // Arrange
        var configurator = _serviceProvider.GetRequiredService<ITestRegistrationConfigurator>();

        // Clear existing rules for a clean state
        configurator.ClearRules();

        // Add a dynamic rule for LastName
        configurator.RuleFor(x => x.LastName)
            .MinimumLength(3)
            .WithMessage("Last name must be at least 3 characters.");

        // Act
        // Exclude Email, but LastName should still be validated
        configurator.Except(x => x.Email);
        configurator.Build();

        // Assert
        var validator = _serviceProvider.GetRequiredService<IFluentValidator<TestRegistrationDto>>();
        var model = new TestRegistrationDto
        {
            Email = null!, // This should be ignored due to Except
            LastName = "K", // Invalid according to MinimumLength rule
            Password = "validPassword1!"
        };
        var result = validator.Validate(model);

        // The Email field should not cause a 'Required' error (due to Except)
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(TestRegistrationDto.Email));

        // The LastName field should still fail validation
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestRegistrationDto.LastName) && e.ErrorMessage.Contains("at least 3 characters"));
    }

    #region Helpers

    interface ITestRegistrationConfigurator : IValidationTypeConfigurator<TestRegistrationDto>
    {
    }

    class TestRegistrationConfigurator(ValidationConfigurator parent, ValidationBehaviorOptions options)
        : ValidationTypeConfigurator<TestRegistrationDto>(parent, options), ITestRegistrationConfigurator
    {
    }

    #endregion
}