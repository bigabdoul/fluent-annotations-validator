---
title: Fluent Configuration API
breadcrumb: FluentAnnotationsValidator > Documentation > Fluent Configuration API
version: v2.0.0-preview.2
---

# Fluent Configuration API

This release introduces a new, more expressive fluent API for configuring validation. It enhances flexibility for complex and conditional validation, providing a more intuitive and powerful developer experience.

## 🚀 Quickstart

### 1\. DTO Annotations

You can continue to use standard `System.ComponentModel.DataAnnotations` on your Data Transfer Objects (DTOs).

```csharp
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;

public class BaseIdentityModel
{
    [Required]
    public virtual string Email { get; set; } = default!;

    [Required, StringLength(20, MinimumLength = 6)]
    public virtual string Password { get; set; } = default!;
}

[ValidationResource(typeof(FluentValidationMessages))]
public class RegisterModel : BaseIdentityModel
{
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    public DateTime? BirthDate { get; set; }
}

public class LoginModel : BaseIdentityModel
{
    public IList<string> Scopes { get; set; } = [];
}
```

### 2\. Localized Messages

Use a .resx file to define localized validation messages with conventional keys (Property_Attribute).

```csharp
public class FluentValidationMessages
{
    public static System.Globalization.CultureInfo? Culture { get; set; }
    public const string Email_Required = "Email is required.";
    public const string Email_NotEmpty = "Email is cannot be empty.";
    public const string Email_EmailAddress = "Email format is invalid.";
    public const string Password_Required = "Password is required.";
    public const string Password_StringLength = "The Password field must be a string with a minimum length of {0} and a maximum length of {1}.";
    public const string Password_Must = "Password must contain at least one digit.";
}
```

### 3\. Configuration

Use the AddFluentAnnotations(...) extension method in your Startup.cs or Program.cs to configure validation rules.

```csharp
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Configuration;

public static partial class FluentValidationUtils
{
    public static IServiceCollection ConfigureFluentAnnotations(this IServiceCollection services)
    {
        services.AddFluentAnnotations(
            // This factory makes the validation exclusively French
            localizerFactory: factory => new(SharedResourceType: typeof(FluentValidationMessages), SharedCulture: CultureInfo.GetCultureInfo("fr")),
            configure: config =>
            {
                // RegisterModel configuration
                var registrationConfig = config.For<RegisterModel>();

                // Add a preemptive rule that overrides any previous configuration for 'Email'
                registrationConfig.Rule(x => x.Email)
                    .Required()
                    .EmailAddress();

                // Add a non-preemptive rule for 'Email' that adds more rules to the property
                registrationConfig.RuleFor(x => x.Email)
                    .When(dto => dto.Email.EndsWith("@example.com"),
                        rule => rule.Must(email => email.Any(char.IsDigit))
                    );

                // Add a preemptive rule that does NOT override previous configuration for 'Password'
                registrationConfig.Rule(x => x.Password,
                    must: BeComplexPassword,
                    RuleDefinitionBehavior.Preserve);

                registrationConfig.RuleFor(x => x.BirthDate)
                    .When(x => x.BirthDate.HasValue, rule => rule.Must(BeAtLeast13));

                registrationConfig.Build();

                // LoginModel configuration
                var loginConfig = config.For<LoginModel>();

                // Non-preemptive rule that retains statically defined constraints (RequiredAttribute).
                // Add a rule to distinguish between admins, and other users. This scenario is:
                // "If the email does not contain the @ symbol, the Scopes collection must contain 'admin';
                // otherwise, Scopes must be empty or contain 'user'."
                loginConfig.RuleFor(x => x.Scopes)
                    .When(IsNotBlankInvalidEmail, scopes => scopes.Must(ContainAdminScopes))
                    .Otherwise(scopes => scopes.Must(BeEmptyOrContainUserScope));

                // This Must(...) method is an alias for When(...) to make the intent clearer.
                loginConfig.RuleFor(x => x.Email)
                    .Must(BeValidEmailAddressIfNotAdmin, rule => rule.EmailAddress());

                loginConfig.Build();
            },
            targetAssembliesTypes: [typeof(RegisterModel)]
        );
    }
    
    private static bool BeAtLeast13(DateTime? date) => DateTime.UtcNow.AddYears(-13) >= date;

    private static bool BeComplexPassword(string password)
    {
        // A regular expression that checks for a complex password.
        // (?=.*[a-z])    - Must contain at least one lowercase letter.
        // (?=.*[A-Z])    - Must contain at least one uppercase letter.
        // (?=.*\d)       - Must contain at least one digit.
        // (?=.*[!@#$%^&*()_+=[{\]};:"'<,>.?/|\-`~]) - Must contain at least one non-alphanumeric character.
        // .              - Matches any character (except newline).

        var passwordRegex = ComplexPasswordRegex();

        return passwordRegex.IsMatch(password);
    }

    private static bool IsNotBlankInvalidEmail(LoginModel m) => !string.IsNullOrWhiteSpace(m.Email) && !m.Email.Contains('@');

    private static bool ContainAdminScopes(HashSet<string> scopes) => scopes.Contains("admin") || scopes.Contains("superuser");

    private static bool BeEmptyOrContainUserScope(HashSet<string> scopes) => scopes.Count == 0 || scopes.Contains("user");

    private static bool BeValidEmailAddressIfNotAdmin(LoginModel user)
    {
        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email)) return true; // invalid, enacts the EmailAddress() validator

        return
            // if it contains an @ symbol, then it must be a valid email address
            email.Contains('@') ||
            // non-administrators definitely need a valid email address
            !string.Equals(email, "admin", StringComparison.OrdinalIgnoreCase) && 
            !string.Equals(email, "superuser", StringComparison.OrdinalIgnoreCase) &&
            !user.Scopes.Contains("admin");
    }

    private static bool BeComplexPassword(string password)
    {
        // A regular expression that checks for a complex password.
        // (?=.*[a-z])    - Must contain at least one lowercase letter.
        // (?=.*[A-Z])    - Must contain at least one uppercase letter.
        // (?=.*\d)       - Must contain at least one digit.
        // (?=.*[!@#$%^&*()_+=[{\]};:"'<,>.?/|\-`~]) - Must contain at least one non-alphanumeric character.
        // .              - Matches any character (except newline).

        var passwordRegex = ComplexPasswordRegex();

        return passwordRegex.IsMatch(password);
    }

    [GeneratedRegex(@"(?:\+?224|00224)?[\s.-]?(?:\d{3}[\s.-]?\d{3}[\s.-]?\d{3}|\d{3}[\s.-]?\d{2}[\s.-]?\d{2}[\s.-]?\d{2})")]
    private static partial Regex GuineaPhoneNumberRegex();

    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).*$")]
    private static partial Regex ComplexPasswordRegex();
}
```

#### 3\.1 No ValidationAttribute annotation on model's member
To ensure the library handles cases where a model's members don't contain any ValidationAttribute annotation, the model must either implement the IFluentValidatable marker interface or pass the extraValidatableTypes parameter to the IServiceCollection.AddFluentAnnotations(...) extension method.

Declare types:

```csharp
using FluentAnnotationsValidator.Abstractions;

public class Product : IFluentValidatable
{
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public bool IsPhysicalProduct { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
}

public class ProductOrder
{
    public string OrderId { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}
```

Add validation rules:

```csharp
services.AddFluentAnnotations(
    configure: config =>
    {
        var productConfigurator = config.For<Product>();
        // Configure Product and Build
        productConfigurator.RuleFor(x => x.ShippingAddress)
            .When(x => x.IsPhysicalProduct, rule =>
            {
                // These rules are evaluated if IsPhysicalProduct is true
                rule.NotEmpty().MaximumLength(100);
            })
            .Otherwise(rule =>
            {
                // This rule will be evaluated if IsPhysicalProduct is false
                rule.Must(address => address == "N/A")
                    .WithMessage("The shipping address for non-physical products must be N/A.");
            });

        productConfigurator.Build();

        // Configure ProductOrder and Build
        var orderConfigurator = config.For<ProductOrder>();
        
        orderConfigurator.RuleFor(x => x.OrderId)
            .NotEmpty()
            .MinimumLength(8);

        orderConfigurator.Build();
    },
    extraValidatableTypes: () => [typeof(ProductOrder)],
    targetAssembliesTypes: [typeof(Product)]
);
```

#### 3\.2 Pre-Validation Value Providers
Pre-validation value providers are a new mechanism to modify or retrieve a member's value before validation. This is useful for data preparation, normalization, initialization, or fetching values from external sources.

Example:

```csharp
services.AddFluentAnnotations(
    configure: config =>
    {
        var productConfigurator = config.For<ProductModel>();

        // BeforeValidation(...) can be called in any order, but only
        // ONCE for this configurator, ProductModel, and ProductId.
        productConfigurator.RuleFor(x => x.ProductId)
            .BeforeValidation(EnsureProductIdInitialized)
            .Required()
            .NotEmpty()
            .ExactLength(36);

        productConfigurator.Build();
    }
);

// Makes sure productId is not blank.
static string? EnsureProductIdInitialized(ProductModel product, MemberInfo member, string? productId)
    => product.ProductId = string.IsNullOrWhiteSpace(productId) ? Guid.NewGuid().ToString() : productId;
```

### 4\. Runtime Validation

Inject IFluentValidator<T> into your services, controllers, or Minimal API endpoints to validate your DTOs.

```csharp
app.MapPost("/register", async (RegisterModel dto, IFluentValidator<RegisterModel> validator) =>
{
    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // Proceed with registration
});
```
