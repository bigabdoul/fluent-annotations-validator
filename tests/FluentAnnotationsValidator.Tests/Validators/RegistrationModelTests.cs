using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Messages;
using FluentAnnotationsValidator.Tests.Models;
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

                // Non-preemptive rule (preserves all previously registered rules)
                configurator.RuleFor(x => x.Password)
                    .Must(BeComplexPassword)
                    .WithMessage(ConventionValidationMessages.Password_MustValidation);

                configurator.Build();

            }, targetAssembliesTypes: typeof(TestRegistrationDto))
            .BuildServiceProvider();
    }

    [Theory]
    [InlineData("Abc1234!", true)]
    [InlineData("password", false)]
    [InlineData("PASSWORD", false)]
    [InlineData("12345678", false)]
    [InlineData("Abcdefgh", false)]
    [InlineData("Abc1234", false)] // too short
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
}