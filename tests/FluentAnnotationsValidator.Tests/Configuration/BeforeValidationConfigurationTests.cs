using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentAnnotationsValidator.Tests.Configuration;

using Models;
using Validators;

public class BeforeValidationConfigurationTests
{
    private static readonly Predicate<BeforeValidationTestDto> AlwaysValidate = _ => true;

    private readonly ServiceCollection _services = new();
    private MockValidationRuleGroupRegistry _mockOptions;
    private FluentTypeValidator<BeforeValidationTestDto> _configurator;

    private IFluentValidator<BeforeValidationTestDto> Validator =>
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<BeforeValidationTestDto>>();

    public BeforeValidationConfigurationTests()
    {
        _services.AddFluentAnnotationsValidators
        (
            new ConfigurationOptions
            {
                ConfigureValidatorRoot = config => _configurator = config.For<BeforeValidationTestDto>(),
                ConfigureRegistry = options => _mockOptions = new(options)
            }
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
        var ruleBuilder = new ValidationRuleBuilder<BeforeValidationTestDto, int>(pendingRule, ValidationRuleGroupRegistry.Default);

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

    [Fact]
    public void BeforeValidation_OnSameMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Id = 1 };
        var configurator = _configurator;
        var ruleBuilder2 = configurator.RuleFor(x => x.Id).Required();

        // Act
        var ruleBuilder1 = configurator.RuleFor(x => x.Id)
            .Required()
            .BeforeValidation((i, m, v) => i.Id = 2);

        // Attempt to assign a second delegate
        ruleBuilder2.BeforeValidation((i, m, v) => i.Id = 3);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            configurator.Build();
        });

        // Verify the exception message
        Assert.Contains("A pre-validation value provider delegate can only be assigned once per member", exception.Message);
    }

    [Fact]
    public void BeforeValidation_OnConfiguratorThenRuleBuilder_ThrowsInvalidOperationException()
    {
        // Arrange
        static object? configure(BeforeValidationTestDto instance, MemberInfo m, object? memberValue) 
            => instance.Name = memberValue?.ToString()?.Trim();

        // Act
        // First, configure a delegate via the IValidationTypeConfigurator
        _configurator.Rule(x => x.Name, RuleDefinitionBehavior.Preserve)
            .Required()
            .BeforeValidation(configure);

        // Then, try to configure another delegate on the same member via a rule builder
        _configurator.RuleFor(x => x.Name)
            .Required()
            .BeforeValidation((instance, m, memberValue) => instance.Name = memberValue?.ToString()?.ToUpper());

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _configurator.Build();
        });

        Assert.Contains("A pre-validation value provider delegate can only be assigned once per member", exception.Message);
    }

    [Fact]
    public void BeforeValidation_WhenAlreadyInRegistry_ThrowsInvalidOperationException()
    {
        // Arrange
        var testDto = new BeforeValidationTestDto { Name = "Existing Name" };
        var member = typeof(BeforeValidationTestDto).GetProperty(nameof(BeforeValidationTestDto.Name))!;

        // Simulate a rule already existing in the registry with a pre-validation delegate
        var existingRule = new ValidationRule()
        {
            Member = member,
            ConfigureBeforeValidation = (instance, member, memberValue) => "Modified Value"
        };

        // This is the core of the test, simulating a rule from a previous Build() call
        _mockOptions.Options.AddRule(member, existingRule);

        // Attempt to configure a new pre-validation delegate on the same member
        _configurator.RuleFor(x => x.Name)
            .Required()
            .BeforeValidation((instance, m, memberValue) => "New Modified Value");

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            _configurator.Build();
        });

        // Assert
        Assert.Contains("A pre-validation value provider delegate can only be assigned once per member", exception.Message);
    }

    [Fact]
    public void WhenAsync_SetsConditionCorrectly()
    {
        // Arrange
        static async Task<bool> asyncCondition(BeforeValidationTestDto dto, CancellationToken cancellationToken = default) => await Task.FromResult(dto.Name!.Length > 5);

        // Act
        _configurator.Rule(d => d.Name)
            .Required()
            .WhenAsync(asyncCondition);

        var pendingRule = _configurator.GetCurrentRule();

        // Assert
        Assert.NotNull(pendingRule);
        Assert.NotNull(pendingRule.AsyncCondition);
        Assert.Equal(asyncCondition, pendingRule.AsyncCondition);
    }
}
