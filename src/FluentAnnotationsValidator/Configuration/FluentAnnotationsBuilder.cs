using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Configuration;

public sealed class FluentAnnotationsBuilder(IServiceCollection services, ValidationBehaviorOptions options)
{
    public IServiceCollection Services { get; init; } = services;
    public ValidationBehaviorOptions Options { get; init; } = options;
}
