## FluentAnnotationsValidator.Runtime

FluentAnnotationsValidator.Runtime is the execution engine of the FluentAnnotationsValidator ecosystem. It powers dynamic validation pipelines by discovering attributes, composing rules, and executing validations at runtime. Designed for extensibility, localization, and seamless integration with dependency injection, this package is ideal for building culture-aware, fluent validation systems in .NET.

---

### What It Does

- **Attribute Discovery**  
  Parses `[ValidationAttribute]` and custom annotations like `MustAttribute`, `ComparisonAttribute`, and `CollectionValidationAttribute`.

- **Rule Composition**  
  Builds validation rules dynamically using interfaces like `IValidationRuleBuilder`, `IValidationRuleGroup`, and `IFluentTypeValidator`.

- **Localization Support**  
  Resolves localized error messages via `StringLocalizerFactoryResult` and `TypeUtils`.

- **Runtime Configuration**  
  Configurable via `ConfigurationOptions`, with support for fluent overrides, async rules, and conditional logic.

---

### Installation

```bash
dotnet add package FluentAnnotationsValidator.Runtime
```

---

### Project Structure

#### Annotations
Custom runtime attributes for advanced validation scenarios:
- `MustAttribute` – Inline predicate-based validation
- `ComparisonAttribute` – Cross-property comparison with operators
- `CollectionAttribute`, `CollectionAsyncAttribute` – Collection-level rules
- `AsyncValidationAttribute` – Asynchronous validation support

#### Extensions
Fluent APIs for rule composition and validator access:
- `FluentTypeValidatorExtensions`
- `ValidationRuleBuilderExtensions`

#### Interfaces
Core abstractions for validators, rule groups, and configurators:
- `IFluentTypeValidator`, `IFluentTypeValidatorRoot`
- `IValidationRuleBuilder`, `IValidationRuleGroup`, `IValidationRuleGroupRegistry`
- `IValidationConfigurator`, `IValidationTypeConfigurator`
- `ICollectionRule`, `IValidationRuleInternal`

#### Core Components
- `FluentTypeValidator`, `FluentTypeValidatorRoot` – Central runtime validators
- `ValidationRule`, `ValidationRule<T>` – Rule definitions
- `ValidationRuleBuilder` – Fluent rule construction
- `ValidationRuleGroup`, `ValidationRuleGroupList`, `ValidationRuleGroupRegistry` – Rule grouping and registry
- `PendingRule` – Deferred rule resolution
- `RuleDefinitionBehavior` – Controls rule discovery behavior
- `FluentValidationException` – Exception for validation failures
- `TypeUtils` – Reflection and localization helpers
- `ConfigurationOptions` – DI and runtime configuration
- `FluentAnnotationsBuilder` – Entry point for configuring validation services

---

### Getting Started

```csharp
services.AddFluentAnnotationsValidators(options =>
{
    options.ExtraValidatableTypesFactory = () => [typeof(MyModel)];
    options.ConfigureLocalization = loc => loc.ResourcesPath = "Resources";
});
```

```csharp
var validator = serviceProvider.GetRequiredService<IFluentValidator<MyModel>>();
var result = validator.Validate(new MyModel());
```

---

### Advanced Features

- Conditional rule activation
- Async validation support
- Culture-aware error messages
- Fluent overrides for `[ValidationAttribute]`
- Extensible rule registry for custom scenarios

---

### Related Packages

- [FluentAnnotationsValidator.Annotations](https://www.nuget.org/packages/FluentAnnotationsValidator.Annotations) – Custom attributes
- [FluentAnnotationsValidator.Core](https://www.nuget.org/packages/FluentAnnotationsValidator.Core) – Fluent rule builders

---

### License

MIT

---

### Contributing

We welcome contributions! Please see the [contributor guide](https://github.com/your-org/FluentAnnotationsValidator/blob/main/CONTRIBUTING.md) for details.
