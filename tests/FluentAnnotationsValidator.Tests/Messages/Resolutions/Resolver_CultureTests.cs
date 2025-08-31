using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;
using static TestHelpers;

public class Resolver_CultureTests
{
    [Fact]
    public void Formats_Message_Using_Provided_Culture()
    {
        // Arrange
        var frenchCulture = ValidationMessages.Culture = CultureInfo.GetCultureInfo("fr-FR");

        var rule = new ConditionalValidationRule(
            dto => true,
            resourceKey: nameof(ValidationMessages.Password_Range), // conventional key: Property_Attribute
            resourceType: typeof(ValidationMessages),
            culture: frenchCulture
        );

        var (min, max) = (6, 20);
        var attr = new RangeAttribute(min, max);
        var expectedMessage = string.Format(rule.Culture, ValidationMessages.Password_Range, min, max);
        var resolver = GetMessageResolver<ValidationMessages>(expectedMessage);

        // Act
        var resolvedMessage = resolver.ResolveMessage(typeof(TestLoginDto), nameof(TestLoginDto.Password), attr, rule);

        resolvedMessage.Should().NotBeNull();
        resolvedMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Inits_FLuentValidation_Using_Provided_Culture()
    {
        var services = new ServiceCollection();

        var dto = new TestLoginDto("email@example.com", ""); // 1 error: password required

        // Act
        services.AddFluentAnnotations
        (
            localizerFactory: factory => new(typeof(ValidationMessages), CultureInfo.GetCultureInfo("fr-FR")),
            targetAssembliesTypes: typeof(TestLoginDto)
        );

        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IFluentValidator<TestLoginDto>>();

        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse("The password is required.");
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TestLoginDto.Password) && e.ErrorMessage == ValidationMessages.Password_Required);
    }
}
