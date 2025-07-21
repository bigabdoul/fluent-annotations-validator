# FluentAnnotationsValidator – Changelog

All notable changes to this project will be documented in this file.

## [1.1.0] - 2025-07-21

### Solution Structure

- Removed legacy `AspNetCore.Tests` project; unified test coverage under `FluentAnnotationsValidator.Tests`
- Renamed `ServiceCollectionExtensions` to `ValidatorServiceCollectionExtensions` to better reflect scope
- Relocated DI extensions to `FluentAnnotationsValidator.Extensions.ValidatorServiceCollectionExtensions`
- Removed outdated AspNetCore workflow to emphasize framework-neutral architecture
- Broad refactoring of internal folders and naming for clarity, discoverability, and contributor experience

### Added

- Introduced `IValidationTypeConfigurator<T>` for fluent per-type conditional validation
- Enabled chaining methods: `.When(...)`, `.And(...)`, `.Except(...)`, `.AlwaysValidate(...)`
- Added metadata overrides: `.WithMessage(...)`, `.WithKey(...)`, `.Localized(...)`
- Fluent transitions via `.For<TNext>()` and finalization with `.Build()`
- Buffered rule registration for safer message chaining

### Internal

- Enhanced `ValidationBehaviorOptions.AddCondition(...)` to support metadata and typed lambda expressions
- Refactored `ConditionalValidationRule` to encapsulate predicate and metadata

### Tests

- Full unit test coverage for all fluent configurator methods (`ValidationTypeConfiguratorTests`)
- Introduced reusable test assertions via `ValidationAssertions`

### Docs

- Added `/docs/configuration/fluent.md` guide for developer onboarding
- Updated XML comments on public methods and interfaces


## [v1.0.7] - 2025-07-20
### 🧠 Message Resolution Refactor
- Refactored `ValidationMessageResolver` into DI-enabled service
- Introduced `IValidationMessageResolver` interface for customization
- Added `TargetModelType` to `PropertyValidationInfo` for resource context
- Updated documentation to reflect new resolution precedence and extension points

## [v1.0.6] - 2025-07-19
### 🚀 Finalized Release
- Fixed CI workflow path to core `.csproj`
- Bumped `.csproj` versions to `1.0.6` to match tag
- Published both packages successfully to NuGet
- Verified Source Link, symbols, icon, and metadata
- Added install badges and NuGet listing polish

## [v1.0.5] - 2025-07-19
### 🛠 Package Stabilization
- Attempted full publish including core and ASP.NET packages
- One publish failed due to csproj misreferencing in CI workflow
- TargetFramework confirmed as net8.0 across all projects

## [v1.0.4] - 2025-07-19
### 🚀 NuGet Publish (ASP.NET Core)
- FluentAnnotationsValidator.AspNetCore published successfully to NuGet
- Core package failed due to lingering net9.0 SDK references in workflow or project
- Added Source Link diagnostics and confirmed deterministic settings

## [v1.0.3] - 2025-07-19
### 🧠 Developer Experience Upgrade
- Embedded icon.png into `.nupkg`
- Added Source Link support via `Microsoft.SourceLink.GitHub`
- Published `.snupkg` symbols for full step-through debugging
- Enabled deterministic builds and reproducible packages
- Cleaned warnings (`NU1604`) with version pinning

## [v1.0.2] - 2025-07-19
### ⚠️ Broken SDK Target
- Build failed due to unsupported `net9.0` in `.csproj`
- Downgraded targets to net8.0 for CI compatibility
- Unpublished (internal fix iteration only)

## [v1.0.1] - 2025-07-19
### 🧹 Version Bump Recovery
- NuGet version conflict on `v1.0.0`
- Retagged to `v1.0.1` but inherited SDK targeting issues

## [v1.0.0] - 2025-07-19
### 🎉 Initial Release
- Reflection-powered bridge between `DataAnnotations` and `FluentValidation`
- DI integration for ASP.NET Core and Minimal APIs
- Convention-based localization and message resolution
