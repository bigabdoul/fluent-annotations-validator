using Demo.Application.DTOs;
using Demo.Infrastructure.Entities;
using FluentAnnotationsValidator;
using FluentAnnotationsValidator.AspNetCore;
using Microsoft.AspNetCore.Identity;

namespace Demo.WebApi.Endpoints;

public static class AuthEndpoints
{
    /// <summary>
    /// Registers authentication-related endpoints for user registration and login.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> used to configure the application's routing.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// This method maps:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>/register</c> – Uses <c>MapValidPost</c> to automatically perform fluent validation on <see cref="RegisterModel"/> before execution.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>/login</c> – Uses manual injection of <see cref="IFluentValidator{T}"/> to validate <see cref="LoginModel"/> with more control.</description>
    ///   </item>
    /// </list>
    /// See <see cref="FluentEndpointRouteBuilderExtensions"/> for details on <c>
    /// <see cref="FluentEndpointRouteBuilderExtensions.MapValidPost{T}(IEndpointRouteBuilder, string, Delegate)"/>, 
    /// <see cref="FluentEndpointRouteBuilderExtensions.MapValidPut{T}(IEndpointRouteBuilder, string, Delegate)"/>, and 
    /// <see cref="FluentEndpointRouteBuilderExtensions.MapValidPatch{T}(IEndpointRouteBuilder, string, Delegate)"/></c>.
    /// </remarks>
    public static IEndpointRouteBuilder AddAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // This mapping method automatically performs fluent validation before getting hit.
        // See the extension methods in Demo.WebApi.Endpoints.EndpointRouteBuilderExtensions.
        app.MapValidPost<RegisterModel>("/register", async (RegisterModel model, UserManager<ApplicationUser> userManager, ILogger<RegisterModel> logger) =>
        {
            var user = new ApplicationUser
            {
                Id = model.Id = Guid.NewGuid().ToString(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                logger.LogInformation("Registration succeeded. User ID is {Id}", model.Id);
                return Results.Ok(model);
            }

            var errors = result.Errors.Select(e => $"Code: {e.Code} ; Description: {e.Description}");
            logger.LogError("User registration failed for {Email}. Details: {Errors}", model.Email, errors);

            return Results.Ok(new { error = "Registration failed" });
        })
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status200OK);

        // When using regular mapping, we must inject IFluentValidator<T> for more validation control.
        app.MapPost("/login", async (LoginModel model, IFluentValidator<LoginModel> validator, ILogger<LoginModel> logger) =>
        {
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                logger.LogWarning("Login failed for {Email}", model.Email);
                return Results.BadRequest(result.Errors);
            }
            logger.LogInformation("Login succeeded for {Email}", model.Email);
            return Results.Ok(new { Token = Guid.NewGuid() });
        })
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
