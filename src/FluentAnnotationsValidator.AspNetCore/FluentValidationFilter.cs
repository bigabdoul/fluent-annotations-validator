using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.AspNetCore;

/// <summary>
/// An endpoint filter that performs fluent validation on the incoming request model of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the request model to validate.</typeparam>
public class FluentValidationFilter<T> : IEndpointFilter
{
    /// <summary>
    /// Invokes the filter to validate the request model before executing the endpoint handler.
    /// </summary>
    /// <param name="context">The context for the current endpoint invocation.</param>
    /// <param name="next">The delegate representing the next filter or endpoint handler in the pipeline.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that returns a <c>400 Bad Request</c> with validation errors if the model is invalid,
    /// or proceeds to the next handler if validation succeeds.
    /// </returns>
    /// <remarks>
    /// This filter resolves <see cref="IFluentValidator{T}"/> from the request services and applies validation to the first argument of type <typeparamref name="T"/>.
    /// </remarks>
    public virtual async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetRequiredService<IFluentValidator<T>>();
        var model = context.Arguments.OfType<T>().FirstOrDefault();

        if (model is null)
            return Results.BadRequest("Invalid payload.");

        var result = await validator.ValidateAsync(model);

        if (!result.IsValid)
        {
            return Results.BadRequest(result.Errors);
        }

        return await next(context);
    }
}
