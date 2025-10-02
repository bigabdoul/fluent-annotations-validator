## FluentAnnotationsValidator.AspNetCore

FluentAnnotationsValidator.AspNetCore provides seamless integration between FluentAnnotationsValidator and ASP.NET Core minimal APIs. It enables automatic model validation via endpoint filters, reducing boilerplate and improving developer experience.

---

### Features

- Automatic validation for POST, PUT, PATCH endpoints
- Plug-and-play with `IFluentValidator<T>`
- Compatible with ASP.NET Core DI

---

### Installation

```bash
dotnet add package FluentAnnotationsValidator.AspNetCore
```

---

### Usage

#### Register endpoints with automatic validation:

```csharp
app.MapValidPost<RegisterModel>("/register", (RegisterModel model) =>
{
    return Results.Ok(model);
});
```

#### Manual validation with full control:

```csharp
app.MapPost("/login", async (LoginModel model, IFluentValidator<LoginModel> validator) =>
{
    var result = await validator.ValidateAsync(model);
    return result.IsValid ? Results.Ok() : Results.BadRequest(result.Errors);
});
```

---

### Setup

Register the validator and filters in your `Program.cs` or `Startup.cs`:

```csharp
services.AddFluentAnnotationsValidator();
```

---

### Documentation

- [FluentAnnotationsValidator.AspNetCore](https://www.nuget.org/packages/FluentAnnotationsValidator.AspNetCore)

---

### Contributing

We welcome contributions! Please see the [contributor guide](https://github.com/bigabdoul/fluent-annotations-validator/blob/main/CONTRIBUTING.md).

---

### License

MIT
