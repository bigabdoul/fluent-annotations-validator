using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Internals.Reflection;
using FluentAnnotationsValidator.Messages;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
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
        var rule = new ConditionalValidationRule(
            dto => true,
            ResourceKey: nameof(ValidationMessages.Password_Range), // conventional key: Property_Attribute
            ResourceType: typeof(ValidationMessages)
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
        var resolvedMessage = new ValidationMessageResolver(new ValidationBehaviorOptions())
            .ResolveMessage(info.DeclaringType, info.Member.Name, attr, rule);

        Assert.Equal(expectedMessage, resolvedMessage);
    }

    [Fact]
    public void Inits_FLuentValidation_Using_Provided_Culture()
    {
        var services = new ServiceCollection();

        var dto = new TestLoginDto("email@example.com", ""); // 1 error: password required
        var culture = CultureInfo.GetCultureInfo("fr-FR");

        // Act
        services.AddFluentAnnotations(
            configureBehavior: options =>
            {
                options.CommonCulture = culture;
                options.CommonResourceType = typeof(ValidationMessages);
            }
        );

        var validator = services.GetValidator<TestLoginDto>();

        var result = validator.Validate(dto);

        // Assert
        Assert.Multiple
        (
            () => Assert.False(result.IsValid),
            () => Assert.Equal
                (
                    // error message must match localized version
                    GetValidationMessage<TestLoginDto, RequiredAttribute>(x => x.Password, culture),
                    result.Errors.FirstOrDefault
                    (
                        e => e.PropertyName == nameof(TestLoginDto.Password)
                    )?.ErrorMessage
                )
        );
    }

    [Fact]
    public void Should_Apply_Scoped_Localization()
    {
        // Configure
        ValidationBehaviorOptions behavior = default!;
        var englishCulture = CultureInfo.GetCultureInfo("en-US"); // English UI culture

        var services = new ServiceCollection()
            .AddFluentAnnotations
            (
                culture: englishCulture,
                resourceType: typeof(ValidationMessages),
                configure: builder => builder
                    .For<TestLoginDto>()
                        .When(x => x.Email, model => model.Role != "Admin") // Non-Admin role members must provide a valid email
                    .Build(),
                configureBehavior: options => behavior = options
            );

        var rule = behavior.FindRule<TestLoginDto, RequiredAttribute>(x => x.Email);
        var resolver = GetMessageResolver(behavior);
        var resolvedMessage = resolver.ResolveMessage(typeof(TestLoginDto), nameof(TestLoginDto.Email), rule.Attribute!, rule);
        var validator = services.GetValidator<TestLoginDto>();

        // invalid model: email is missing but is required when the role is not "Admin"
        var dto = new TestLoginDto(null!, Password: "Pass123", Role: "User");

        // Act
        var result = validator.Validate(dto);

        Assert.Multiple
        (
            () => Assert.False(result.IsValid),
            () => Assert.Contains(result.Errors, e => e.PropertyName == "Email"),

            // shouldn't be equal because of culture mismatch (ValidationMessages.Email_Required
            // returns the French message because of the current thread's UI culture)
            () => Assert.NotEqual(GetValidationMessage<TestLoginDto, RequiredAttribute>(x => x.Email, "fr-FR"), resolvedMessage),

            // should match the English message, as specified in culture ("en-US")
            () => Assert.Equal(GetValidationMessage<TestLoginDto, RequiredAttribute>(x => x.Email, englishCulture), resolvedMessage)
        );
    }
}
