using Microsoft.AspNetCore.Http;

namespace FluentAnnotationsValidator.AspNetCore;

/// <summary>
/// Middleware that performs fluent validation on incoming HTTP requests of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The request model type to validate.</typeparam>
public class FluentValidationMiddleware<T>(RequestDelegate next) where T : class
{
    /// <summary>
    /// Invokes the middleware to validate the incoming request model before passing control to the next delegate.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="validator">The fluent validator for the request model type <typeparamref name="T"/>.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the middleware pipeline continues or terminates due to validation failure.
    /// </returns>
    /// <remarks>
    /// This middleware reads the request body as JSON and validates it using <see cref="IFluentValidator{T}"/>.
    /// If the model is null, it returns a <c>400 Bad Request</c>.
    /// If validation fails, it returns a <c>422 Unprocessable Entity</c> with the validation errors.
    /// Otherwise, it invokes the next middleware in the pipeline.
    /// </remarks>
    public virtual async Task InvokeAsync(HttpContext context, IFluentValidator<T> validator)
    {
        var model = await context.Request.ReadFromJsonAsync<T>();
        if (model is null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid payload.");
            return;
        }

        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(result.Errors);
            return;
        }

        await next(context);
    }
}
