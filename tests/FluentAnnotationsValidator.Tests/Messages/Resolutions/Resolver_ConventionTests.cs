using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;

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
        var options = new ValidationBehaviorOptions { UseConventionalKeys = true, SharedResourceType = typeof(ConventionValidationMessages) };

        var localizerFactoryMock = MockStringLocalizerFactory<ConventionValidationMessages>(ConventionValidationMessages.Email_Required);

        // 3. Create the resolver with the mocked factory
        var resolver = new ValidationMessageResolver(options, localizerFactoryMock);

        // Act
        var msg = resolver.ResolveMessage(typeof(LoginDtoWithResource), EmailName, attr);

        // Assert
        Assert.Equal(ConventionValidationMessages.Email_Required, msg);
    }

    [Fact]
    public void Skips_Convention_If_Disabled()
    {
        // Arrange
        var attr = new RequiredAttribute();
        var options = new ValidationBehaviorOptions();
        var rule = new ConditionalValidationRule(
            dto => true,
            useConventionalKeys: false
        );

        // Mock IStringLocalizerFactory, even though it won't be used in this test path
        var localizerFactoryMock = new Mock<IStringLocalizerFactory>();

        var resolver = new ValidationMessageResolver(options, localizerFactoryMock.Object);

        // Act
        var actualMessage = resolver.ResolveMessage(typeof(LoginDtoWithResource), EmailName, attr, rule);

        // Assert
        Assert.Equal(expected: attr.FormatErrorMessage(EmailName), actualMessage);
    }
}
