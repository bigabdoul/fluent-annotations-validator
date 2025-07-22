using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
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
        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!,
            TargetModelType = typeof(TestLoginDto)
        };

        // Act
        var resolvedMessage = new ValidationMessageResolver().ResolveMessage(info, attr, rule);

        Assert.Equal(expectedMessage, resolvedMessage);
    }
}
