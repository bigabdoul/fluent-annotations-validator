## Release Notes – FluentAnnotationsValidator.AspNetCore

### v2.0.0-rc.1.0.0 – Release Candidate

First release candidate of version 2 of FluentAnnotationsValidator.AspNetCore!

#### Features

- `MapValidPost<T>()`, `MapValidPut<T>()`, `MapValidPatch<T>()` for automatic validation
- `FluentValidationFilter<T>` for endpoint-level validation
- Integration with `IFluentValidator<T>` via DI
- Compatible with ASP.NET Core minimal APIs
- Works with custom attributes from `FluentAnnotationsValidator.Annotations`

#### Dependencies

- FluentAnnotationsValidator.Core
- Microsoft.AspNetCore.Http
- Microsoft.Extensions.DependencyInjection
