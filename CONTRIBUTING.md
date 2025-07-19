# 🤝 Contributing to FluentAnnotationsValidator

Welcome! We're excited to have you contribute. Whether you're fixing a bug, improving documentation, or adding new validation logic — every bit helps.

## 🚀 Getting Started

1. Clone the repository  
```bash
git clone https://github.com/bigabdoul/fluent-annotations-validator.git
```

2. Restore and build the solution  
```bash
dotnet restore
dotnet build
```

3. Run tests  
```bash
dotnet test
```

---

## 🧪 Adding New Features

- Add new validation logic in `DataAnnotationsValidator.cs`
- Extend test coverage in `FluentAnnotationsValidator.Tests`
- Keep behavior consistent across `.resx` and static resources

---

## 📘 Documentation

If you add a feature, please consider updating:

- `/docs/` (architecture, customization, etc.)
- `README.md` if applicable

---

## 🔧 Code Style

- Follow clean architecture principles
- Keep PRs focused and modular
- Write testable, well-documented code

---

## 📦 NuGet Packaging

We aim for:

- Deterministic builds
- Source Link embedded
- `.snupkg` for full debugging

PRs that affect the packaging pipeline should include full CI verification.

---

## 📝 Changelog

Add an entry to `CHANGELOG.md` under `[Unreleased]` or create a new version block.

---

Thanks for helping improve FluentAnnotationsValidator! We're building not just software — but developer clarity, productivity, and joy.

Then commit:

```bash
touch CONTRIBUTING.md
# Add contents
git add CONTRIBUTING.md
git commit -m "Add contributing guide"
git push origin main
```
