using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using FluentAnnotationsValidator.Tests.Models;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FluentAnnotationsValidator.Tests.Validators;

public class ValidationTypeConfiguratorTests
{
    private readonly ServiceCollection _services = new();
    private readonly MockValidationConfigurator _mockParentConfigurator;
    private MockValidationBehaviorOptions _mockOptions;
    private ValidationTypeConfigurator<ValidationTypeConfiguratorTestModel> _configurator;

    private IValidator<ValidationTypeConfiguratorTestModel> Validator => 
        _services.BuildServiceProvider().GetRequiredService<IValidator<ValidationTypeConfiguratorTestModel>>();

    public ValidationTypeConfiguratorTests()
    {
        _services.AddFluentAnnotations
        (
            config => _configurator = config.For<ValidationTypeConfiguratorTestModel>(),
            configureBehavior: options => _mockOptions = new(options),
            typeof(ValidationTypeConfiguratorTestModel)
        );
        
        if (_mockOptions is null)
        {
            var provider = _services.BuildServiceProvider();
            var options = provider.GetRequiredService<ValidationBehaviorOptions>();
            _mockOptions = new(options);
        }

        _mockParentConfigurator = new(_mockOptions.Options);
        _configurator ??= new ValidationTypeConfigurator<ValidationTypeConfiguratorTestModel>(_mockParentConfigurator, _mockOptions.Options);
    }

    [Fact]
    public void Rule_ShouldCommitPreviousRuleAndStartNewOne()
    {
        // Arrange
        var configurator = _configurator;

        // Should override existing attribute-based rules for that member (Name).
        // It currently has these: [Required, MinLength(5)]
        configurator.Rule(x => x.Name).NotEmpty();

        // Act
        // This call to 'Rule' should commit the NotEmpty rule for Name and start a new rule for Email.
        configurator.Rule(x => x.Email).Required(when: m => m.Age > 10);//.When(m => m.Age > 10);

        // Build to process the rules. The 'Build' method will call CommitCurrentRule for the final rule.
        configurator.Build();

        // Assert
        // We should have rules registered for both 'Name' and 'Email'.
        _mockOptions.AddedRules.Should().HaveCount(2);
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Name");
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Email");
    }

    [Fact]
    public void Rule_WithChainedMethods_ShouldAttachMultipleAttributes()
    {
        // Arrange
        var configurator = _configurator;

        // Act

        // Initially one rule with 2 attributes: [Required, Length(50)]
        configurator.Rule(x => x.Email).Required().MaximumLength(50); 
        
        configurator.Build();

        // Assert
        // The result is a set of 2 rules, one for each attribute
        _mockOptions.AddedRules.Count(r => r.Member.Name == "Email").Should().Be(2);
        var ruleForEmail = _mockOptions.AddedRules.First(r => r.Member.Name == "Email").Rule;
        ruleForEmail.Attribute.Should().NotBeNull();
        ruleForEmail.Attribute.GetType().Should().Be(typeof(RequiredAttribute));

        ruleForEmail = _mockOptions.AddedRules.Last(r => r.Member.Name == "Email").Rule;
        ruleForEmail.Attribute.Should().NotBeNull();
        ruleForEmail.Attribute.GetType().Should().Be(typeof(LengthCountAttribute));
    }


    [Fact]
    public void Build_ShouldRegisterBothStaticAndDynamicAttributes()
    {
        // Arrange
        // The ValidationTypeConfiguratorTestModel has static [Required] and [MinLength(5)] on the Name property.
        // We will add a dynamic rule.
        var configurator = _configurator;
        configurator.Rule(x => x.Age).Required();

        // Act
        configurator.Build();

        // Assert
        // The Build method should find the static attributes and the dynamic ones.
        // Static attributes for 'Name':
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Name" && r.Rule.Attribute is RequiredAttribute);
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Name" && r.Rule.Attribute is MinLengthAttribute);

        // Dynamic attribute for 'Age':
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Age" && r.Rule.Attribute is RequiredAttribute);
    }

    [Fact]
    public void Build_DynamicRulesShouldBeAttachedBeforeFallbacks()
    {
        // Arrange
        var configurator = _configurator;

        // The Name property has 2 static attributes: [Required, MinLength(5)]
        // The dynamic Rule call should override the default fallback rule for this member.

        // This one should prevail and replace the static attributes.
        configurator.Rule(x => x.Name)
            //.Preserve()
            .Length(2, 10);

        // Act
        configurator.Build();

        // Assert
        var nameRules = _mockOptions.AddedRules.Where(r => r.Member.Name == "Name").ToList();

        // We should have only one rule (the static attributes should be gone);
        // only the dynamic one should exist.
        nameRules.Count.Should().Be(1);
        nameRules.Should().NotContain(r => r.Rule.Attribute is RequiredAttribute);
        nameRules.Should().NotContain(r => r.Rule.Attribute is MinLengthAttribute);
        nameRules.Should().Contain(r => r.Rule.Attribute is LengthCountAttribute);
    }

    [Fact]
    public void Build_ShouldNotRegisterFallbackRulesForOverriddenMembers()
    {
        // Arrange
        var configurator = _configurator;
        configurator.Rule(x => x.Email).NotEmpty();

        // Act
        configurator.Build();

        // Assert
        // The Build method should not register any fallback rules for the `Email` member because it was overridden.
        _mockParentConfigurator.RegisteredActions.Should().BeEmpty();
    }

    [Fact]
    public void Rule_ShouldHonorRequired_WhenChainedCondition_IsMet()
    {
        // Arrange
        var configurator = _configurator;
        var model = new ValidationTypeConfiguratorTestModel { Email = "user@example.com", Age = 20 };

        // Get rid of any rules for Name
        //configurator.RemoveRulesFor(x => x.Name);

        // Or clear all rules for the model to be sure
        configurator.ClearRules();

        // Act
        configurator.Rule(x => x.Email)
            .EmailAddress()
            .Required()
            .When(m => m.Age >= 18); // condition is chained

        configurator.Build();

        // Assert
        // We should have rules registered for 'Email' with the specified conditions.
        _mockOptions.AddedRules.Should().HaveCount(2);
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Email" && r.Rule.Attribute is RequiredAttribute);

        Validator.Validate(model).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Rule_ShouldNotHonorRequired_WhenChainedCondition_IsNotMet()
    {
        // Arrange
        // Initialize with empty rules
        var configurator = _configurator.ClearRules();
        var model = new ValidationTypeConfiguratorTestModel { Email = null, Age = 15 };

        // Act
        configurator.Rule(x => x.Email)
            .EmailAddress()
            .Required()
            .When(m => m.Age >= 18); // condition is chained
        configurator.Build();

        // Assert
        // We should have rules registered for 'Email' with the specified conditions.
        _mockOptions.AddedRules.Should().HaveCount(2);
        _mockOptions.AddedRules.Should().Contain(r => r.Member.Name == "Email" && r.Rule.Attribute is RequiredAttribute);

        // Invalid email should fail; however, the age is under the requirement
        Validator.Validate(model).IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenAndOtherwise_WhenConditionIsTrue_ShouldExecuteWhenRules()
    {
        // Arrange
        var configurator = _configurator.ClearRules();
        var model = new ValidationTypeConfiguratorTestModel
        {
            Age = 13,
            IsPhysicalProduct = true,
            ShippingAddress = "" // Invalid according to the 'When' block
        };

        // Act
        configurator.RuleFor(x => x.ShippingAddress)
            .When(x => x.IsPhysicalProduct, rule =>
                rule
                    .NotEmpty().WithMessage("Empty shipping address is disallowed.")
                    .MaximumLength(100).WithMessage("The shipping address cannot exceed 100 characters.")
            )
            .Otherwise(rule => rule.Must(address => address == "N/A"));
            //.Otherwise(rule => rule.NotEmpty()); // What does that mean?

        // What does this imply?
        //configurator.RuleFor(x => x.ShippingAddress).When(x => x.Age < 18, rule => rule.Empty());

        configurator.Build();

        // Assert
        var validationResult = Validator.Validate(model);
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle(e => e.PropertyName == "ShippingAddress" && e.ErrorMessage.Contains("is disallowed"));
    }

    [Fact]
    public void WhenAndOtherwise_WhenConditionIsFalse_ShouldExecuteOtherwiseRules()
    {
        // Arrange
        var configurator = _configurator;
        configurator.ClearRules();

        var model = new ValidationTypeConfiguratorTestModel
        {
            IsPhysicalProduct = false,
            ShippingAddress = "Some other address" // Invalid according to the 'Otherwise' block
        };

        // Act
        configurator.RuleFor(x => x.ShippingAddress)
            .When(x => x.IsPhysicalProduct, rule =>
            {
                // These rules are not evaluated (IsPhysicalProduct = false)
                rule.NotEmpty().MaximumLength(100);
            })
            .Otherwise(rule =>
            {
                // This rule will be executed since it negates IsPhysicalProduct
                rule.Must(address => address == "N/A")
                    .WithMessage("The shipping address for non-physical products must be N/A.");
            });

        configurator.Build();

        // Assert
        var validationResult = Validator.Validate(model);
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle(e => e.PropertyName == "ShippingAddress" && e.ErrorMessage.Contains("must be N/A"));
    }
}