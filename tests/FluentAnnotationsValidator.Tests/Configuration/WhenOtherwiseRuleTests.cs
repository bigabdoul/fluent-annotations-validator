using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Configuration;

public class WhenOtherwiseRuleTests
{
    private readonly ServiceCollection _services = new();
    private FluentTypeValidator<ConditionalTestDto> _configurator;

    private IFluentValidator<ConditionalTestDto> Validator =>
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<ConditionalTestDto>>();

    private static bool AlwaysFail<TProp>(TProp? value) => false;
    private static Task<bool> AlwaysFailAsync<TProp>(TProp? _1, CancellationToken _2) => Task.FromResult(false);

    public WhenOtherwiseRuleTests()
    {
        _services.AddFluentAnnotationsValidators(new ConfigurationOptions
        {
            ConfigureValidatorRoot = config => _configurator = config.For<ConditionalTestDto>(),
            ExtraValidatableTypesFactory = () => [typeof(ConditionalTestDto)],
        });

        ArgumentNullException.ThrowIfNull(_configurator);
    }

    [Fact]
    public void Given_WhenConditionIsFalse_Then_OtherwiseRulesShouldRun()
    {
        // Arrange
        var testDto = new ConditionalTestDto { Age = 10, Name = "Alice" };

        _configurator.RuleFor(x => x.Name)
            .When
            (
                x => x.Age != 10, // condition is false, since Age = 10
                rule => rule.Must(AlwaysFail).WithMessage("When rule succeeded.")
            )
            .Otherwise(rule => rule.Must(AlwaysFail).WithMessage("Otherwise rule ran but failed."));

        _configurator.Build();

        // Act
        var errors = Validator.Validate(testDto).Errors;

        // Assert
        Assert.Single(errors);
        Assert.Equal("Otherwise rule ran but failed.", errors[0].ErrorMessage);
    }

    [Fact]
    public void Given_WhenConditionIsTrue_Then_OtherwiseRulesShouldNotRun()
    {
        // Arrange
        var testDto = new ConditionalTestDto { Age = 30, Name = "Bob" };

        _configurator.RuleFor(x => x.Name)
            .When(x => x.Age == 30, rule => rule.Must(AlwaysFail).WithMessage("When rule ran but failed."))
            .Otherwise(rule => rule.Must(AlwaysFail).WithMessage("Otherwise rule ran but failed."));

        _configurator.Build();

        // Act
        var errors = Validator.Validate(testDto).Errors;

        // Assert
        Assert.Single(errors);
        Assert.Equal("When rule ran but failed.", errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Given_WhenAsyncConditionIsFalse_Then_OtherwiseAsyncRulesShouldRun()
    {
        // Arrange
        var testDto = new ConditionalTestDto { Age = 10, Name = "Alice" };

        _configurator.RuleFor(x => x.Name)
            .WhenAsync
            (
                (x, token) => Task.FromResult(x.Age != 10), // condition is false, since Age = 10
                rule => rule.MustAsync(AlwaysFailAsync).WithMessage("WhenAsync rule succeeded.")
            )
            .OtherwiseAsync(builder =>
            {
                builder.MustAsync(AlwaysFailAsync).WithMessage("OtherwiseAsync rule ran but failed.");
                return Task.CompletedTask;
            });

        _configurator.Build();

        // Act
        var errors = await Validator.ValidateAsync(testDto);

        // Assert
        Assert.Single(errors.Errors);
        Assert.Equal("OtherwiseAsync rule ran but failed.", errors.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Given_WhenAsyncConditionIsTrue_Then_OtherwiseAsyncRulesShouldNotRun()
    {
        // Arrange
        var testDto = new ConditionalTestDto { Age = 30, Name = "Bob" };

        _configurator.RuleFor(x => x.Name)
            .WhenAsync
            (
                (x, token) => Task.FromResult(x.Age == 30),
                rule => rule.MustAsync(AlwaysFailAsync).WithMessage("WhenAsync rule ran but failed.")
            )
            .OtherwiseAsync(rule =>
            {
                rule.MustAsync(AlwaysFailAsync).WithMessage("OtherwiseAsync rule ran but failed.");
                return Task.CompletedTask;
            });

        _configurator.Build();

        // Act
        var errors = await Validator.ValidateAsync(testDto);

        // Assert
        Assert.Single(errors.Errors);
        Assert.Equal("WhenAsync rule ran but failed.", errors.Errors[0].ErrorMessage);
    }
}
