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
        var rule = new ConditionalValidationRule(
            dto => true,
            ResourceKey: nameof(FrenchMessages.LengthMessage),
            ResourceType: typeof(FrenchMessages),
            Culture: CultureInfo.GetCultureInfo("fr-FR")
        );

        var attr = new StringLengthAttribute(5);
        var info = new PropertyValidationInfo
        {
            Property = typeof(TestLoginDto).GetProperty(nameof(TestLoginDto.Password))!,
            TargetModelType = typeof(TestLoginDto)
        };

        var resolver = new ValidationMessageResolver();
        var msg = resolver.ResolveMessage(info, attr, rule);

        Assert.Equal("Mot de passe requis: 5 caractères max.", msg);
    }
}
