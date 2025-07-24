using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Configuration;

public sealed class FluentAnnotationsBuilder(IServiceCollection services, ValidationBehaviorOptions options)
{
    public IServiceCollection Services { get; } = services;
    public ValidationBehaviorOptions Options { get; } = options;
}
