using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Validators;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Tests.Configuration;

public class BeforeValidationConfigurationTests
{
    private static readonly Func<BeforeValidationTestDto, bool> AlwaysValidate = _ => true;

    private readonly ServiceCollection _services = new();
    private MockValidationBehaviorOptions _mockOptions;
    private ValidationTypeConfigurator<BeforeValidationTestDto> _configurator;

    private IFluentValidator<BeforeValidationTestDto> Validator =>
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<BeforeValidationTestDto>>();

    public BeforeValidationConfigurationTests()
    {
        _services.AddFluentAnnotations
        (
            configure: config => _configurator = config.For<BeforeValidationTestDto>(),
            configureBehavior: options => _mockOptions = new(options)
        );

        ArgumentNullException.ThrowIfNull(_configurator);
        ArgumentNullException.ThrowIfNull(_mockOptions);
    }

    [Fact]
    public void BeforeValidation_OnTypeConfigurator_AssignsDelegateCorrectly()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Name = "  TestName  " };
        Expression<Func<BeforeValidationTestDto, string?>> memberExpression = dto => dto.Name;
        var member = memberExpression.GetMemberInfo();

        // Simulate a pending rule being created for the configurator
        var pendingRule = new PendingRule<BeforeValidationTestDto>(memberExpression, AlwaysValidate);
        _configurator.SetCurrentRule(pendingRule);

        // Act
        _configurator.BeforeValidation((instance, member, memberValue) => instance.Name = instance.Name?.Trim());

        // Assert
        Assert.NotNull(pendingRule.ConfigureBeforeValidation);
        Assert.Equal("TestName", pendingRule.ConfigureBeforeValidation.Invoke(testDto, member, testDto.Name));
    }

    [Fact]
    public void BeforeValidation_OnTypeConfigurator_ModifiesValueCorrectly()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Name = "  TrimMe  " };
        Expression<Func<BeforeValidationTestDto, string?>> memberExpression = dto => dto.Name;
        var member = memberExpression.GetMemberInfo();

        var pendingRule = new PendingRule<BeforeValidationTestDto>(memberExpression, AlwaysValidate);

        _configurator.SetCurrentRule(pendingRule);

        // Assign the delegate
        _configurator.BeforeValidation((instance, member, memberValue) => ((string?)memberValue)?.Trim());

        // Act
        var result = pendingRule.ConfigureBeforeValidation!.Invoke(testDto, member, testDto.Name);

        // Assert
        Assert.IsType<string?>(result);
        Assert.Equal("TrimMe", result);
    }

    [Fact]
    public void BeforeValidation_OnRuleBuilder_AssignsDelegateCorrectly()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Id = 0 };
        Expression<Func<BeforeValidationTestDto, int>> memberExpression = dto => dto.Id;
        var pendingRule = new PendingRule<BeforeValidationTestDto>(memberExpression, AlwaysValidate);
        var ruleBuilder = new ValidationRuleBuilder<BeforeValidationTestDto, int>(pendingRule);

        // Act
        ruleBuilder.Required().BeforeValidation((instance, m, memberValue) => instance.Id = 123);

        // Assert
        Assert.NotNull(ruleBuilder.GetRules().Last().ConfigureBeforeValidation);
    }

    [Fact]
    public void BeforeValidation_OnRuleBuilder_ModifiesValueCorrectly()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Id = 0 };
        var configurator = _configurator;

        // Act
        configurator.RuleFor(x => x.Id)
            .Required()
            .BeforeValidation((instance, m, memberValue) => 456);

        configurator.Build();

        var result = Validator.Validate(testDto);

        // Assert
        Assert.Equal(456, testDto.Id);
    }

    [Fact]
    public void Validate_WithBeforeValidation_ValueModifiedToPassValidation()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Id = 0 }; // Fails Range check
        var configurator = _configurator;
        var newValidationId = 5;

        // Act
        configurator.RuleFor(x => x.Id)
            .Range(1, int.MaxValue)
            .BeforeValidation((instance, member, memberValue) => instance.Id = newValidationId);

        configurator.Build();

        var result = Validator.Validate(testDto);

        // Assert
        Assert.Empty(result.Errors); // Validation should pass
        Assert.Equal(newValidationId, testDto.Id); // The value should be modified
    }

    [Fact]
    public void Validate_WithBeforeValidation_ValueModifiedToFailValidation()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Name = "ValidName" }; // Initially valid
        var configurator = _configurator;

        // Act
        configurator.RuleFor(x => x.Name)
            .Required()
            .BeforeValidation((instance, member, memberValue) => instance.Name = null); // render Name invalid

        configurator.Build();

        var result = Validator.Validate(testDto);

        // Assert
        Assert.NotEmpty(result.Errors); // Validation should fail
        Assert.Single(result.Errors);
        Assert.Null(testDto.Name); // The value should be modified
    }
}
