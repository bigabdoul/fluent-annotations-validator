using Demo.Infrastructure;
using Demo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

#pragma warning restore IDE0130

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, bool useCookieAuth = false)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=app.db"));

        IdentityBuilder identityBuilder;

        if (useCookieAuth)
        {
            // This configuration - AddIdentity<ApplicationUser, IdentityRole>() - redirects unauthenticated
            // requests to the login page. If you're using cookies (i.e., from a browser), that's fine.

            identityBuilder = services.AddIdentity<ApplicationUser, IdentityRole>(ConfigureIdentityOptions);
        }
        else
        {
            // Use AddIdentityCore<ApplicationUser>(...) for JWT-based authentication:
            // it does not redirect unauthenticated requests; rather, it issues a 401.

            services.AddDataProtection(); // Required for token providers when using minimal identity setup.

            identityBuilder = services.AddIdentityCore<ApplicationUser>(ConfigureIdentityOptions);
        }

        identityBuilder
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    }
}
