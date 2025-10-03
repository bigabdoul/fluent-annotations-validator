using Demo.Application.DTOs;
using Demo.Infrastructure;
using Demo.Infrastructure.Entities;
using FluentAnnotationsValidator;
using FluentAnnotationsValidator.Runtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Demo.WebApi.Validations;

public static partial class FluentRuleDefinitions
{
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).*$")]
    private static partial Regex ComplexPasswordRegex();

    private static ServiceProvider Provider = default!;

    public static IServiceCollection ConfigureFluentAnnotations(this IServiceCollection services)
    {
        services.AddFluentAnnotations(new ConfigurationOptions
        {
            ConfigureValidatorRoot = root =>
            {
                // The using statements ensure Build() is called when done configuring.
                using var register = root.For<RegisterModel>();

                register.RuleFor(x => x.Email)
                    .WhenAsync(IsValidEmail, rule =>
                    {
                        rule.MustAsync(BeUniqueEmail)
                            .WithMessage("A valid email is required.");
                    });

                register.RuleFor(x => x.Password) // Password rule with complex requirement.
                    .Must(pwd => ComplexPasswordRegex().IsMatch(pwd))
                    .WithMessage("The password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

                using var login = root.For<LoginModel>();
                login.Rule(x => x.Password).Required(); // Override password rule and make it a simple requirement.

                // Other configurations
                root.RulesForCatalogs()
                    .RulesForProducts()
                    .RulesForOrders();
            },
            TargetAssembliesTypes = [typeof(LoginModel)],
        });

        Provider = services.BuildServiceProvider();

        return services;
    }

    #region Rules Definitions

    internal static FluentTypeValidatorRoot RulesForCatalogs(this FluentTypeValidatorRoot root)
    {
        using var config = root.For<CatalogModel>();

        config.RuleFor(x => x.UserId)
            .WhenAsync(IsNewEntity, rule =>
            {
                rule.MustAsync(HasExistingUser)
                    .WithMessage(catalog => $"User ID {catalog.UserId} was not found. The catalog '{catalog.Name}' cannot be added.");
            });

        config.RuleFor(x => x)
            .WhenAsync(IsNewEntity, rule =>
            {
                rule.MustAsync(NotBeDuplicateCatalog)
                    .WithMessage(catalog => $"User ID {catalog.UserId} already has a catalog named '{catalog.Name}'.");
            });

        return root;
    }

    internal static FluentTypeValidatorRoot RulesForProducts(this FluentTypeValidatorRoot root)
    {
        using var config = root.For<ProductModel>();

        config.RuleFor(x => x.CatalogId)
            .WhenAsync(IsNewEntity, rule =>
            {
                rule.MustAsync(HasExistingCatalog)
                   .WithMessage(product => $"The product '{product.Name}' does not belong to an existing catalog.");
            });

        config.RuleFor(x => x.UserId)
            .WhenAsync(IsNewEntity, rule =>
            {
                rule.MustAsync(HasExistingUser)
                    .WithMessage(product => $"User ID {product.UserId} was not found. The product '{product.Name}' cannot be added.");
            });

        return root;
    }

    internal static FluentTypeValidatorRoot RulesForOrders(this FluentTypeValidatorRoot root)
    {
        using var config = root.For<OrderModel>();

        config.RuleFor(x => x.UserId)
            .WhenAsync(IsNewEntity, rule =>
            {
                rule.MustAsync(HasExistingUser)
                    .WithMessage(order => $"User ID {order.UserId} was not found. The order for '{order.CustomerName}' cannot be added.");
            });

        config.RuleForEach(x => x.OrderItems)
            .WhenAsync(IsNewEntity, rule =>
            {
                rule.ChildRules(orderItem =>
                {
                    orderItem.RuleFor(x => x.Quantity)
                        .Minimum(0, isExclusive: true)
                        .WithMessage("The minimum quantity for an order item must be at least 1.");

                    orderItem.RuleFor(x => x.UnitPrice)
                        .Minimum(0M)
                        .WithMessage("The price of an item cannot be negative.");

                    orderItem.RuleFor(x => x)
                        .WhenAsync(IsNewEntity, rule =>
                            rule.MustAsync(HasExistingProduct)
                                .WithMessage(item => $"No product found with ID #{item.ProductId}.")
                        );
                });
            });

        return root;
    }

    #endregion

    #region Services

    internal static void GetLogger<T>(Action<ILogger<T>> action) =>
        action.Invoke(Provider.GetRequiredService<ILogger<T>>());

    internal static FluentTypeValidatorRoot GetTypeValidatorRoot()
        => Provider.GetRequiredService<FluentTypeValidatorRoot>();

    #endregion

    #region Helpers

    private static Task<bool> IsValidEmail(RegisterModel x, CancellationToken cancellationToken)
        => Task.FromResult(!string.IsNullOrWhiteSpace(x.Email));

    private static async Task<bool> BeUniqueEmail(string? email, CancellationToken cancellationToken)
    {
        if (email is null) return false;
        var userManager = Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user == null;
    }

    private static Task<bool> IsNewEntity(EntityId x, CancellationToken _) => Task.FromResult(x.Id <= 0L);

    private static async Task<bool> HasExistingUser(string? userId, CancellationToken cancellationToken)
    {
        var db = Provider.GetRequiredService<AppDbContext>();
        var exists = await db.Users.AnyAsync(user => user.Id == userId, cancellationToken);

        if (!exists)
        {
            GetLogger<CatalogModel>(lg => lg.LogWarning("The user with ID {UserId} does not exist.", userId));
        }

        return exists;
    }

    private static async Task<bool> HasExistingCatalog(long catalogId, CancellationToken cancellationToken)
    {
        var db = Provider.GetRequiredService<AppDbContext>();
        var exists = await db.Catalogs.AnyAsync(catalog => catalog.Id == catalogId, cancellationToken);

        if (!exists)
        {
            GetLogger<ProductModel>(lg => lg.LogWarning("The catalog {CatalogId} does not exist.", catalogId));
        }

        return exists;
    }

    private static async Task<bool> HasExistingProduct(OrderItemModel? orderItem, CancellationToken cancellationToken)
    {
        if (orderItem is null) return false;

        var db = Provider.GetRequiredService<AppDbContext>();
        var exists = await db.Products.AnyAsync(product => product.Id == orderItem.ProductId, cancellationToken);

        if (!exists)
        {
            GetLogger<ProductModel>(lg => lg.LogWarning("The product {ProductId} does not exist.", orderItem.ProductId));
        }

        return exists;
    }

    private static async Task<bool> NotBeDuplicateCatalog(CatalogModel? catalog, CancellationToken cancellationToken)
    {
        if (catalog is null) return false;

        var db = Provider.GetRequiredService<AppDbContext>();
        var exists = await db.Catalogs.AnyAsync(c => c.UserId == catalog.UserId && c.Name == catalog.Name, cancellationToken);

        if (exists)
        {
            GetLogger<CatalogModel>(lg =>
                lg.LogWarning("The user with ID {UserId} already has a catalog named '{CatalogName}'.",
                    catalog.UserId, catalog.Name));
        }

        return !exists;
    }

    #endregion
}
