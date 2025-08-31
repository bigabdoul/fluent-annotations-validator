## FluentAnnotationsValidator v1.2.2 – Implicit Rule Resolution & Registry Overhaul

**Release date:** 2025-07-23

### Fixes

- Added support for generating `ConditionalValidationRule`s from `[ValidationAttribute]`s when `.When(...)` is omitted
- Respects `.WithCulture(...)` and `.WithValidationResource(...)` during fallback synthesis
- Centralized metadata resolution via `ValidationTypeConfiguratorBase` and `ValidationConfiguratorRegistry`
- 🛠️ Refactored fallback engine (`IImplicitRuleResolver`) for cleaner orchestration

### Architectural Improvements

- Introduced `ValidationConfiguratorRegistry` as a singleton-style metadata store
- Migrated `ValidationTypeConfigurator<T>` to inherit from non-generic base
- Enhanced configurator introspection for DTO-aware culture and resource resolution

### ⚠ Known Limitations

> 📛 Only one rule per property is currently supported.  
> Multi-attribute decorators (e.g. `[Required]`, `[MinLength]`, `[EmailAddress]`) only emit one message per property.  
> This will be resolved in **v2.0.0**, with support for full rule aggregation and multi-message flows.
