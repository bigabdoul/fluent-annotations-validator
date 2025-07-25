using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;

public class Resolver_CultureTests
{
    [Fact]
    public void Formats_Message_Using_Provided_Culture()
    {
        // Arrange
        var rule = new ConditionalValidationRule(
            dto => true,
            ResourceKey: nameof(ValidationMessages.Password_Range), // conventional key: Property_Attribute
            ResourceType: typeof(ValidationMessages),
            Culture: ValidationMessages.Culture = CultureInfo.GetCultureInfo("fr-FR") // must set the Culture on ValidationMessages
        );

        var (min, max) = (6, 20);
        var attr = new RangeAttribute(min, max);
        var expectedMessage = string.Format(ValidationMessages.Password_Range, min, max);
        var info = new MemberValidationInfo
        {
            Member = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!,
            DeclaringType = typeof(TestLoginDto)
        };

        // Act
        var resolvedMessage = new ValidationMessageResolver(new ValidationBehaviorOptions()).ResolveMessage(info.DeclaringType, info.Member.Name, attr, rule);

        Assert.Equal(expectedMessage, resolvedMessage);
    }

    [Fact]
    public void Inits_FLuentValidation_Using_Provided_Culture()
    {
        var services = new ServiceCollection();

        var dto = new TestLoginDto("email@example.com", ""); // 1 error: password required

        // Act
        services.AddFluentAnnotations(
            configureBehavior: options =>
            {
                options.CommonCulture = CultureInfo.GetCultureInfo("fr-FR");
                options.CommonResourceType = typeof(ValidationMessages);
            }
        );

        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IValidator<TestLoginDto>>();

        var result = validator.Validate(dto);

        // Assert
        Assert.Multiple
        (
            () => Assert.False(result.IsValid),
            () => Assert.Equal
                (
                    // error message must match localized version
                    ValidationMessages.Password_Required,
                    result.Errors.FirstOrDefault
                    (
                        e => e.PropertyName == nameof(TestLoginDto.Password)
                    )?.ErrorMessage
                )
        );
    }
}
