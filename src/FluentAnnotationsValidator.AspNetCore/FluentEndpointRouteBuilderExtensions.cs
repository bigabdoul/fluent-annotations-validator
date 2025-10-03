using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FluentAnnotationsValidator.AspNetCore;

/// <summary>
/// Provides extension methods for mapping HTTP endpoints with automatic FluentAnnotationsValidator integration.
/// </summary>
public static class FluentEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a POST endpoint and applies fluent validation for the specified request type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The request model type to validate.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern to match.</param>
    /// <param name="handler">The delegate to handle the request.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> with validation applied.</returns>
    /// <remarks>
    /// Uses <see cref="FluentValidationFilter{T}"/> to perform validation before invoking the handler.
    /// </remarks>
    public static RouteHandlerBuilder MapValidPost<T>(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapPost(pattern, handler)
            .AddEndpointFilter(new FluentValidationFilter<T>());
    }

    /// <summary>
    /// Maps a PUT endpoint and applies fluent validation for the specified request type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The request model type to validate.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern to match.</param>
    /// <param name="handler">The delegate to handle the request.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> with validation applied.</returns>
    public static RouteHandlerBuilder MapValidPut<T>(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapPut(pattern, handler)
            .AddEndpointFilter(new FluentValidationFilter<T>());
    }

    /// <summary>
    /// Maps a PATCH endpoint and applies fluent validation for the specified request type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The request model type to validate.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">The route pattern to match.</param>
    /// <param name="handler">The delegate to handle the request.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> with validation applied.</returns>
    public static RouteHandlerBuilder MapValidPatch<T>(this IEndpointRouteBuilder endpoints, string pattern, Delegate handler)
    {
        return endpoints.MapPatch(pattern, handler)
            .AddEndpointFilter(new FluentValidationFilter<T>());
    }
}

