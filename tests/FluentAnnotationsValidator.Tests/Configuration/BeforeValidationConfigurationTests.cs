using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Tests.Models;
using Moq;
using System.Linq.Expressions;

namespace FluentAnnotationsValidator.Tests.Configuration;

public class BeforeValidationConfigurationTests
{
    private readonly Mock<ValidationBehaviorOptions> _mockOptions;
    private readonly Mock<ValidationConfigurator> _mockParent;
    private readonly ValidationTypeConfigurator<BeforeValidationTestDto> _configurator;

    private static readonly Func<BeforeValidationTestDto, bool> AlwaysValidate = _ => true;

    public BeforeValidationConfigurationTests()
    {
        _mockOptions = new Mock<ValidationBehaviorOptions>();
        _mockParent = new Mock<ValidationConfigurator>(_mockOptions.Object);
        _configurator = new ValidationTypeConfigurator<BeforeValidationTestDto>(_mockParent.Object, _mockOptions.Object);
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
        _configurator.BeforeValidation((instance, member, memberValue) => {
            instance.Name = instance.Name?.Trim();
            return memberValue;
        });

        // Assert
        Assert.NotNull(pendingRule.ConfigureBeforeValidation);
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

        // add a rule
        ruleBuilder.When(dto => dto.Id <= 0, builder => builder.Required());

        // Act
        ruleBuilder.BeforeValidation((instance, m, memberValue) => 123);

        // Assert
        Assert.NotNull(ruleBuilder.GetRules().Last().ConfigureBeforeValidation);
    }

    [Fact]
    public void BeforeValidation_OnRuleBuilder_ModifiesValueCorrectly()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Id = 0 };
        Expression<Func<BeforeValidationTestDto, int>> memberExpression = dto => dto.Id;
        var member = memberExpression.GetMemberInfo();
        var pendingRule = new PendingRule<BeforeValidationTestDto>(memberExpression, AlwaysValidate);
        var ruleBuilder = new ValidationRuleBuilder<BeforeValidationTestDto, int>(pendingRule);

        // add a rule
        //ruleBuilder.When(dto => dto.Id <= 0, builder => builder.Range(1, 1000));
        ruleBuilder.When(dto => dto.Id <= 0, builder => builder.Required());

        // Assign the delegate
        ruleBuilder.BeforeValidation((instance, m, memberValue) => 456);

        var rule = ruleBuilder.GetRules().Last();

        // Act
        var result = rule.ConfigureBeforeValidation!.Invoke(testDto, member, testDto.Id);

        // Assert
        Assert.Equal(456, result);
    }
}
