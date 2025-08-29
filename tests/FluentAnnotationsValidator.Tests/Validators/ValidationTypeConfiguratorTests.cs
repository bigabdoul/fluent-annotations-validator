using FluentAnnotationsValidator.Configuration;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Metadata;
using FluentAnnotationsValidator.Tests.Models;
using FluentAnnotationsValidator.Tests.Resources;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FluentAnnotationsValidator.Tests.Validators;
using static TestHelpers;

public class ValidationTypeConfiguratorTests
{
    private readonly ServiceCollection _services = new();
    private readonly MockValidationConfigurator _mockParentConfigurator;
    private MockValidationBehaviorOptions _mockOptions;
    private ValidationTypeConfigurator<ValidationTypeConfiguratorTestModel> _configurator;
    private ValidationTypeConfigurator<TestProductModel> _productConfigurator;

    private IFluentValidator<ValidationTypeConfiguratorTestModel> Validator => 
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<ValidationTypeConfiguratorTestModel>>();
    private IFluentValidator<TestProductModel> ProductValidator =>
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<TestProductModel>>();

    public ValidationTypeConfiguratorTests()
    {
        _services.AddFluentAnnotations
        (
            config =>
            {
                _configurator = config.For<ValidationTypeConfiguratorTestModel>();
                _productConfigurator = config.For<TestProductModel>();
            },
            configureBehavior: options => _mockOptions = new(options),
            targetAssembliesTypes: typeof(ValidationTypeConfiguratorTestModel)
        );
        
        if (_mockOptions is null)
        {
            var provider = _services.BuildServiceProvider();
            var options = provider.GetRequiredService<ValidationBehaviorOptions>();
            _mockOptions = new(options);
        }

        _mockParentConfigurator = new(_mockOptions.Options);

        ArgumentNullException.ThrowIfNull(_configurator);
        ArgumentNullException.ThrowIfNull (_productConfigurator);
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
        // We should have rules registered for both 'Name' and 'Email', and 'ConfirmEmail' already present.
        var addedRules = _mockOptions.AddedRules;
        addedRules.Should().HaveCount(3);
        addedRules.Should().Contain(r => r.Member.Name == "Name");
        addedRules.Should().Contain(r => r.Member.Name == "Email");
    }

    [Fact]
    public void Rule_WithChainedMethods_ShouldAttachMultipleAttributes()
    {
        // Arrange
        var configurator = _configurator;

        // Act

        // Initially one rule with 2 attributes: [Required, EmailAddress]
        configurator.Rule(x => x.Email).Required().MaximumLength(50); 
        
        configurator.Build();

        // Assert
        // The result is a set of 2 rules, one for each attribute
        var addedRules = _mockOptions.AddedRules;
        addedRules.Count(r => r.Member.Name == "Email").Should().Be(2);
        var ruleForEmail = addedRules.First(r => r.Member.Name == "Email").Rule;
        ruleForEmail.Attribute.Should().NotBeNull();
        ruleForEmail.Attribute.GetType().Should().Be(typeof(RequiredAttribute));

        ruleForEmail = addedRules.Last(r => r.Member.Name == "Email").Rule;
        ruleForEmail.Attribute.Should().NotBeNull();
        ruleForEmail.Attribute.GetType().Should().Be(typeof(MaxLengthAttribute));
    }


    [Fact]
    public void Build_ShouldRegisterBothStaticAndDynamicAttributes()
    {
        // Arrange
        // The ValidationTypeConfiguratorTestModel has static [Required] and [MinLength(5)] on the Name property.
        // We will add a dynamic rule.
        var configurator = _configurator;
        configurator.Rule(x => x.Age, RuleDefinitionBehavior.Preserve).Required();

        // Act
        configurator.Build();

        // Assert
        var addedRules = _mockOptions.AddedRules;
        // The Build method should find the static attributes and the dynamic ones.
        // Static attributes for 'Name':
        addedRules.Should().Contain(r => r.Member.Name == "Name" && r.Rule.Attribute is RequiredAttribute);
        addedRules.Should().Contain(r => r.Member.Name == "Name" && r.Rule.Attribute is MinLengthAttribute);

        // Dynamic attribute for 'Age':
        addedRules.Should().Contain(r => r.Member.Name == "Age" && r.Rule.Attribute is RequiredAttribute);
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
        nameRules.Should().Contain(r => r.Rule.Attribute is Length2Attribute);
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
        var addedRules = _mockOptions.AddedRules;
        // We should have rules registered for 'Email' with the specified conditions.
        addedRules.Should().HaveCount(2);
        addedRules.Should().Contain(r => r.Member.Name == "Email" && r.Rule.Attribute is RequiredAttribute);

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
        var addedRules = _mockOptions.AddedRules;
        // We should have rules registered for 'Email' with the specified conditions.
        addedRules.Should().HaveCount(2);
        addedRules.Should().Contain(r => r.Member.Name == "Email" && r.Rule.Attribute is RequiredAttribute);

        // Invalid email should fail; however, the age is under the requirement
        Validator.Validate(model).IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenAndOtherwise_WhenConditionIsTrue_ShouldExecuteWhenRules()
    {
        // Arrange
        var configurator = _productConfigurator.ClearRules();
        var model = new TestProductModel
        {
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
        var validationResult = ProductValidator.Validate(model);
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle(e => e.PropertyName == "ShippingAddress" && e.ErrorMessage.Contains("is disallowed"));
    }

    [Fact]
    public void WhenAndOtherwise_WhenConditionIsFalse_ShouldExecuteOtherwiseRules()
    {
        // Arrange
        var configurator = _productConfigurator;
        configurator.ClearRules();

        var model = new TestProductModel
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
        var validationResult = ProductValidator.Validate(model);
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle(e => e.PropertyName == "ShippingAddress" && e.ErrorMessage.Contains("must be N/A"));
    }

    [Fact]
    public void Rule_ShouldSupportCompareAttribute()
    {
        // Arrange
        var configurator = _configurator;
        var model = new ValidationTypeConfiguratorTestModel
        {
            Name = "John Doe",
            Email = "john@example.com",
            ConfirmEmail = "john@example.com"
        };

        // Act
        configurator.Rule(x => x.Email)
            //.Required()
            //.EmailAddress()
            .Compare(x => x.ConfirmEmail).WithMessage("The {0} property must match the {1} value.")
            ;

        configurator.Build();

        // Assert
        var validationResult = Validator.Validate(model);
        validationResult.IsValid.Should().BeTrue();

        // Now change ConfirmEmail to something else
        model.ConfirmEmail = "doe@example.com";
        validationResult = Validator.Validate(model);
        validationResult.IsValid.Should().BeFalse();
    }

    // Test member-based attribute removal
    [Fact]
    public void Rule_ShouldBeRemoved_UsingSpecificAttribute()
    {
        // Arrange
        var configurator = _configurator;
        var model = new ValidationTypeConfiguratorTestModel
        {
            Name = null, // This should trigger the RequiredAttribute
            Email = "john@example.com", // Email also has RequiredAttribute
            ConfirmEmail = "john@example.com"
        };

        // Act

        // Only remove the RequiredAttribute for Name
        configurator.RemoveRulesFor(x => x.Name, typeof(RequiredAttribute));

        configurator.Build();

        // Assert

        // The RequiredAttribute for Name should be removed, so the model should be valid now.
        var validationResult = Validator.Validate(model);
        validationResult.IsValid.Should().BeTrue();

        // The Email should still have the RequiredAttribute
        model.Email = null;
        validationResult = Validator.Validate(model);
        validationResult.IsValid.Should().BeFalse();

        _mockOptions.AddedRules.Should().Contain(r => 
            r.Member.Name == nameof(model.Email) && 
            r.Rule.Attribute is RequiredAttribute);
    }

    [Theory]
    [InlineData("Abc1234!", true)]
    [InlineData("password", false)]
    [InlineData("PASSWORD", false)]
    [InlineData("12345678", false)]
    [InlineData("Abcdefgh", false)]
    [InlineData("Abc1234", false)] // too short
    public void Preemptive_Rule_ComplexPassword_Should_Return_CorrectResult(string password, bool validResult)
    {
        // Arrange
        var configurator = _configurator.ClearRules(); // Focus only on the Password property

        configurator.Rule(x => x.Password, must: BeComplexPassword)
            .WithMessage(ConventionValidationMessages.Password_Must)
            .Build();

        var model = new ValidationTypeConfiguratorTestModel { Password = password, Email = "test@example.com" };

        // Act
        var result = Validator.Validate(model);

        // Assert
        if (validResult)
        {
            result.IsValid.Should().BeTrue();
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(ValidationTypeConfiguratorTestModel.Password) &&
                e.ErrorMessage == ConventionValidationMessages.Password_Must);
        }
    }
}