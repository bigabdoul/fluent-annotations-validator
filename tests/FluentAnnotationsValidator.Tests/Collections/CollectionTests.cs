using FluentAnnotationsValidator.Tests.Models;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Collections;

public class CollectionTests
{
    private readonly ServiceCollection _services = new();
    private FluentTypeValidator<ConditionalTestDto> _configurator;

    private IFluentValidator<ConditionalTestDto> Validator =>
        _services.BuildServiceProvider().GetRequiredService<IFluentValidator<ConditionalTestDto>>();

    private static bool AlwaysPass<TProp>(TProp? value) => true;

    public CollectionTests()
    {
        _services.AddFluentAnnotationsValidators(new ConfigurationOptions
        {
            ConfigureValidatorRoot = config => _configurator = config.For<ConditionalTestDto>(),
            ExtraValidatableTypesFactory = () => [typeof(ConditionalTestDto)],
        });

        ArgumentNullException.ThrowIfNull(_configurator);
    }

    [Fact]
    public void RuleForEach_Is_Recursively_Applied_To_Parent_And_ChildRules_With_Collections()
    {
        // Arrange
        static bool StartWithItemA(string? itemName) => true == itemName?.StartsWith("Item A");

        // Configure a rule that is applied only when the condition is false.
        _configurator.RuleForEach(x => x.Items)
            .When(x => 
            x.Age >= 21, rules =>
            {
                rules.ChildRules(item => item.RuleFor(x => x.ItemName).NotEmpty());
                rules.Must(AlwaysPass).WithMessage("This rule should not run.");
            })
            .Otherwise(rules =>
            {
                rules.ChildRules(item =>
                {
                    item.RuleFor(x => x.ItemName)
                        .Required()
                        .NotEmpty()
                        .Must(StartWithItemA).WithMessage(x => $"{x.ItemName} failed validation.");

                    item.RuleForEach(x => x.Products)
                        .When(x => x.ItemName == "Item A but different", product =>
                        {
                            product
                                .NotEmpty()
                                .ChildRules(prod =>
                                {
                                    prod.RuleFor(p => p.ProductId)
                                        .NotEmpty()
                                        .ExactLength(36).WithMessage("Product Id must be exactly 36 chars long."); // Fail: Value has 32 chars

                                    prod.RuleForEach(p => p.Orders)
                                        .NotEmpty()
                                        .ChildRules(order =>
                                        {
                                            order.RuleFor(c => c.OrderId)
                                                .Required()
                                                .NotEmpty()
                                                .ExactLength(36).WithMessage("Order Id must be exactly 36 chars long.");

                                            order.RuleFor(c => c.ProductId)
                                                .Required()
                                                .NotEmpty()
                                                .ExactLength(36).WithMessage("Product Id for order must be exactly 36 chars long."); ;

                                            order.RuleFor(c => c.Quantity)
                                                .Range(1, 1000).WithMessage("Quantity must be between 1 and 1000.");
                                        });
                                });
                        });
                });
            });

        _configurator.Build();

        // Act
        var errors = Validator.Validate(CreateCollectionTestDto()).Errors;

        // Assert
        DoAssertions(errors);
    }

    [Fact]
    public async Task RuleForEach_Is_Recursively_Applied_To_Parent_And_ChildRules_With_Collections_Asynchronous()
    {
        // Arrange
        static bool StartWithItemA(string? itemName) => true == itemName?.StartsWith("Item A");
        static Task<bool> IsOver21(ConditionalTestDto x, CancellationToken cancel) => Task.Run(() => x.Age >= 21);
        
        // Configure a rule that is applied only when the condition is false.
        _configurator.RuleForEach(x => x.Items)
            .WhenAsync(IsOver21, rules =>
            {
                rules.ChildRules(item => item.RuleFor(x => x.ItemName).NotEmpty());
                //rules.Must(AlwaysPass).WithMessage("This rule should not run.");
            })
            .OtherwiseAsync(rules =>
            {
                rules.ChildRules(item =>
                {
                    item.RuleFor(x => x.ItemName)
                        .Required()
                        .NotEmpty()
                        .Must(StartWithItemA).WithMessage(x => $"{x.ItemName} failed validation.");

                    item.RuleForEach(x => x.Products)
                        .When(x => x.ItemName == "Item A but different", product =>
                        {
                            product
                                .NotEmpty()
                                .ChildRules(prod =>
                                {
                                    prod.RuleFor(p => p.ProductId)
                                        .NotEmpty()
                                        .ExactLength(36).WithMessage("Product Id must be exactly 36 chars long."); // Fail: Value has 32 chars

                                    prod.RuleForEach(p => p.Orders)
                                        .NotEmpty()
                                        .ChildRules(order =>
                                        {
                                            order.RuleFor(c => c.OrderId)
                                                .Required()
                                                .NotEmpty()
                                                .ExactLength(36).WithMessage("Order Id must be exactly 36 chars long.");

                                            order.RuleFor(c => c.ProductId)
                                                .Required()
                                                .NotEmpty()
                                                .ExactLength(36).WithMessage("Product Id for order must be exactly 36 chars long."); ;

                                            order.RuleFor(c => c.Quantity)
                                                .Range(1, 1000).WithMessage("Quantity must be between 1 and 1000.");
                                        });
                                });
                        });
                });

                return Task.CompletedTask;
            });

        _configurator.Build();

        // Act
        var result = await Validator.ValidateAsync(CreateCollectionTestDto());
        var errors = result.Errors;

        // Assert
        DoAssertions(errors);
    }

    private static void DoAssertions(List<FluentValidationFailure> errors)
    {
        errors.Should().HaveCount(13);

        // ───────────── Items Level ─────────────

        // Error 1: Items[1].ItemName fails Must(StartWithItemA)
        errors.Should().ContainPath("Items[1].ItemName", collectionIndex: 1);

        // Errors 2, 3, 4: Items[2].ItemName fails Required, NotEmpty, Must(StartWithItemA)
        errors.Should().ContainPath("Items[2].ItemName", collectionIndex: 2);
        errors.Should().Contain(e => e.CollectionIndex == 2 && e.PropertyName == nameof(ConditionalTestItemDto.ItemName) && e.ErrorMessage.Contains("required"));
        errors.Should().Contain(e => e.CollectionIndex == 2 && e.PropertyName == nameof(ConditionalTestItemDto.ItemName) && e.ErrorMessage.Contains("not be empty"));
        errors.Should().Contain(e => e.CollectionIndex == 2 && e.PropertyName == nameof(ConditionalTestItemDto.ItemName) && e.ErrorMessage.Contains("failed validation"));

        // Errors 5, 6, 7: Items[3].ItemName fails Required, NotEmpty, Must(StartWithItemA)
        errors.Should().ContainPath("Items[3].ItemName", collectionIndex: 3);
        errors.Should().Contain(e => e.CollectionIndex == 3 && e.PropertyName == nameof(ConditionalTestItemDto.ItemName) && e.ErrorMessage.Contains("required"));
        errors.Should().Contain(e => e.CollectionIndex == 3 && e.PropertyName == nameof(ConditionalTestItemDto.ItemName) && e.ErrorMessage.Contains("not be empty"));
        errors.Should().Contain(e => e.CollectionIndex == 3 && e.PropertyName == nameof(ConditionalTestItemDto.ItemName) && e.ErrorMessage.Contains("failed validation"));

        // ───────────── Products Level ─────────────

        // Error 8: Items[4].Products[0].ProductId fails ExactLength
        errors.Should().ContainPath("Items[4].Products[0].ProductId", collectionIndex: 0, parentIndex: 4);
        errors.Should().Contain(e => e.CollectionIndex == 0 && e.ParentCollectionIndex == 4 && e.PropertyName == nameof(TestProductModel.ProductId) && e.ErrorMessage.Contains("exactly 36"));

        // ───────────── Orders Level ─────────────

        // Errors 9, 10, 11: Items[4].Products[0].Orders[1].OrderId fails Required, NotEmpty, ExactLength
        errors.Should().ContainPath("Items[4].Products[0].Orders[1].OrderId", collectionIndex: 1, parentIndex: 0);
        errors.Should().Contain(e => e.CollectionIndex == 1 && e.ParentCollectionIndex == 0 && e.PropertyName == nameof(ProductOrderModel.OrderId) && e.ErrorMessage.Contains("required"));
        errors.Should().Contain(e => e.CollectionIndex == 1 && e.ParentCollectionIndex == 0 && e.PropertyName == nameof(ProductOrderModel.OrderId) && e.ErrorMessage.Contains("not be empty"));
        errors.Should().Contain(e => e.CollectionIndex == 1 && e.ParentCollectionIndex == 0 && e.PropertyName == nameof(ProductOrderModel.OrderId) && e.ErrorMessage.Contains("exactly 36"));

        // Error 12: Items[4].Products[0].Orders[1].ProductId fails ExactLength
        errors.Should().ContainPath("Items[4].Products[0].Orders[1].ProductId", collectionIndex: 1, parentIndex: 0);
        errors.Should().Contain(e => e.CollectionIndex == 1 && e.ParentCollectionIndex == 0 && e.PropertyName == nameof(ProductOrderModel.ProductId) && e.ErrorMessage.Contains("exactly 36"));

        // Error 13: Items[4].Products[0].Orders[2].Quantity fails Range
        errors.Should().ContainPath("Items[4].Products[0].Orders[2].Quantity", collectionIndex: 2, parentIndex: 0);
        errors.Should().Contain(e => e.CollectionIndex == 2 && e.ParentCollectionIndex == 0 && e.PropertyName == nameof(ProductOrderModel.Quantity) && e.ErrorMessage.Contains("between 1 and 1000"));
    }

    private static ConditionalTestDto CreateCollectionTestDto()
    {
        // This configuration has 13 validation errors.
        return new ConditionalTestDto
        {
            Age = 10, // Condition causes the 'Otherwise' branch to execute.
            Items =
            [
                /*Items[0]*/new() { ItemName = "Item A" }, // No Error @ index 0: ItemName starts with "Item A" 
                /*Items[1]*/new() { ItemName = "Item B" }, // Error 1 @ index 1: Must(StartWithItemA)
                /*Items[2]*/new() { ItemName = null! }, // Errors 2, 3, and 4 @ index 2: Required, NotEmpty, and Must(StartWithItemA)
                /*Items[3]*/new() { ItemName = string.Empty }, // Errors 5, 6 and 7 @ index 3: Required, NotEmpty, and Must(StartWithItemA)
                /*Items[4]*/new()
                {
                    /*Items[4].*/ItemName = "Item A but different",
                    /*Items[4].*/Products =
                    [
                        /*Items[4].Products[0]*/new()
                        {
                            // Error 8: ExactLength must be 36 chars; is 32 chars long because ToString("n") strips away the dashes.
                            /*Items[4].Products[0].*/ProductId = Guid.NewGuid().ToString("n"),
                            /*Items[4].Products[0].*/Orders =
                            [
                                // Pass
                                /*Items[4].Products[0].Orders[0]*/new() { OrderId = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Quantity = 10 },
                                /*Items[4].Products[0].Orders[1]*/new()
                                {
                                    /*Items[4].Products[0].Orders[1].*/OrderId = string.Empty, // Errors 9, 10, and 11: Required, NotEmpty, and ExactLength (36 chars) not met
                                    /*Items[4].Products[0].Orders[1].*/ProductId = Guid.NewGuid().ToString("n"), // Error 12: ExactLength 32 chars, requires exactly 36
                                    /*Items[4].Products[0].Orders[1].*/Quantity = 5
                                },
                                // Error 13: Range - Quantity not in range [1, 1000]
                                /*Items[4].Products[0].Orders[2]*/new() { OrderId = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Quantity = -20 },
                            ]
                        }
                    ]
                },
            ]
        };
    }

}

static class FluentValidationFailureAssertions
{
    /// <summary>
    /// Asserts that the collection contains a validation failure with the specified path and index metadata.
    /// </summary>
    /// <param name="assertions">The FluentAssertions wrapper.</param>
    /// <param name="expectedPath">The expected ItemPath value.</param>
    /// <param name="collectionIndex">The expected CollectionIndex.</param>
    /// <param name="parentIndex">Optional ParentCollectionIndex.</param>
    public static AndConstraint<GenericCollectionAssertions<FluentValidationFailure>> ContainPath(
        this GenericCollectionAssertions<FluentValidationFailure> assertions,
        string expectedPath,
        int collectionIndex,
        int? parentIndex = null)
    {
        return assertions.Contain(e =>
            e.CollectionIndex == collectionIndex &&
            (parentIndex == null || e.ParentCollectionIndex == parentIndex) &&
            e.CustomState != null &&
            ((IDictionary<string, object>)e.CustomState).ContainsKey("ItemPath") &&
            ((IDictionary<string, object>)e.CustomState)["ItemPath"].ToString() == expectedPath);
    }
}
