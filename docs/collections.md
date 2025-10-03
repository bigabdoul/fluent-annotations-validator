---
title: Customization & Extensibility
breadcrumb: FluentAnnotationsValidator > Documentation > Customization & Extensibility
version: v2.0.0-rc.1.0.0
---

# Collections

## `RuleForEach` for Collection Validation

The `RuleForEach` method is a new addition that enables you to apply validation rules to each item within a collection. This is particularly useful for validating complex nested models, as it allows you to define rules for properties inside a list or array.

When combined with `When` and `Otherwise`, `RuleForEach` becomes a powerful tool for creating highly conditional validation logic on collections. The inner rules defined within a `ChildRules` block are applied to each item that passes the outer condition.

**Example**

```csharp
var services = new ServiceCollection();

services.AddFluentAnnotations(new ConfigurationOptions
{
    ConfigureValidatorRoot = config =>
    {
        using var configurator = config.For<ConditionalTestDto>();

        // Configure a rule that is applied only when the condition is false.
        configurator.RuleForEach(x => x.Items)
            .When(x => x.Age >= 21, rules =>
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
    },
    ExtraValidatableTypesFactory = () => [typeof(ConditionalTestDto)],
});

```

### Retrieve an instance of IFluentValidator and validate a model.

```csharp
// This configuration has 13 validation errors.
var testDto = new ConditionalTestDto
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
```

The `IFluentValidator<T>` service usually comes from the DI registration pipeline. For instance, in a controller action or a minimal API endpoint.

```csharp
var validator => services.BuildServiceProvider().GetRequiredService<IFluentValidator<ConditionalTestDto>>();

var result = validator.Validate(testDto);

// Do something with the result:
if (!result.IsValid)
{
    foreach (var err in result.Errors)
    {
        //...
    }
}
```

This example demonstrates how you can build a deeply nested validation structure. The `RuleForEach` on `Products` applies rules to each product within the `Items` collection, and a subsequent `RuleForEach` on `Orders` validates each order within a specific product. This allows for a very granular, clean, and powerful validation strategy.
