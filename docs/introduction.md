# Introduction

### What is Fluent Annotations Validator?

**Fluent Annotations Validator** is a lightweight and powerful validation library for .NET applications. It solves a common dilemma in software development: whether to use simple, declarative validation attributes or a more flexible, programmatic fluent API. Our library elegantly combines both approaches, giving you the best of both worlds.

Unlike popular alternatives like FluentValidation, you **do not need to inherit from an `AbstractValidator<T>` for each type** you want to validate. This design choice streamlines your codebase and keeps your validation logic separate from your data models.

This tool allows you to link complex, fluent validation rules to your data models using simple attributes. The result is a clean and maintainable codebase where your validation logic is kept separate from your DTOs, adhering to the Single Responsibility Principle.

-----

### The Core Philosophy

The library's core philosophy is to provide a validation system that is both **declarative and extensible**. By using attributes, you can quickly define common validation requirements (like `[Required]` or `[StringLength]`). For more complex scenarios, you can use the fluent API to define powerful, conditional rules.

Fluent Annotations Validator automatically discovers and executes all validation rules associated with a DTO, whether they are defined by attributes or by the fluent API. This two-tiered approach ensures that your models remain clean while your validation logic stays robust and easy to manage.

-----

### Key Features

  * **Unified Validation:** It runs both attribute-based rules (`[Required]`, `[StringLength]`) and fluent rules from a single, unified validation pipeline.
  * **Pre-Validation Hooks:** It enables data manipulation and cleansing **before** validation occurs, such as automatically trimming whitespace from strings.
  * **Extensible and Flexible:** You can easily create custom validation rules to support any business logic.
  * **Centralized Configuration:** All validation rules can be configured in a single, centralized location, making your application's validation logic discoverable and maintainable.
  * **Built for Dependency Injection:** Designed from the ground up to integrate seamlessly with ASP.NET Core and other DI-enabled frameworks.

-----

### How It Works

The validation process follows a clear two-step pipeline. When a DTO is validated, the library first processes all static attributes to perform a quick initial check. Then, it executes any fluent rules you have configured, including powerful conditional logic. This ensures that your data is thoroughly validated from multiple angles.
