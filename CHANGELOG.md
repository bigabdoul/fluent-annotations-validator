# FluentAnnotationsValidator – Changelog

All notable changes to this project will be documented in this file.

## [v1.0.6] - 2025-07-19
### 🔧 Fixes & Workflow Enhancements
- Corrected CI workflow path to core `.csproj` (publish pipeline was failing)
- Verified test project `.NET SDK` targets (net8.0) and removed all net9.0 references
- Confirmed deterministic build settings across all projects
- Validated `.snupkg` symbol publishing and Source Link embed

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
