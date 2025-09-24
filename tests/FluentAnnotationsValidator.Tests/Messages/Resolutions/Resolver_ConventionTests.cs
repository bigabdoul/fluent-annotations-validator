using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Tests.Messages.Resolutions;
using static TestHelpers;

public class Resolver_ConventionTests
{
    const string EmailName = nameof(LoginDtoWithResource.Email);

    [Fact]
    public void Resolves_Using_Convention_When_Enabled()
    {
        // Arrange
        var attr = new RequiredAttribute();
        var registry = GlobalRegistry.Default;
        registry.UseConventionalKeys = true;
        registry.SharedResourceType = typeof(ConventionValidationMessages);
        registry.SharedCulture = ConventionValidationMessages.Culture = CultureInfo.CurrentCulture;
        var localizerFactoryMock = MockStringLocalizerFactory<ConventionValidationMessages>(ConventionValidationMessages.Email_Required);

        // 3. Create the resolver with the mocked factory
        var resolver = new ValidationMessageResolver(registry, localizerFactoryMock);

        // Act
        var msg = resolver.ResolveMessage(new LoginDtoWithResource(), EmailName, attr);

        // Assert
        Assert.Equal(ConventionValidationMessages.Email_Required, msg);
    }

    [Fact]
    public void Skips_Convention_If_Disabled()
    {
        // Arrange
        var attr = new RequiredAttribute();
        
        var rule = new ValidationRule
        {
            UseConventionalKeys = false
        };

        // Mock IStringLocalizerFactory, even though it won't be used in this test path
        var localizerFactoryMock = new Mock<IStringLocalizerFactory>();

        var resolver = new ValidationMessageResolver(new GlobalRegistry(), localizerFactoryMock.Object);

        // Act
        var actualMessage = resolver.ResolveMessage(new LoginDtoWithResource(), EmailName, attr, rule);

        // Assert
        Assert.Equal(expected: attr.FormatErrorMessage(EmailName), actualMessage);
    }
}
