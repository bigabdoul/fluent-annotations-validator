using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator;

public static class FluentAnnotationsConfiguratorExtensions
{
    public static ValidationConfigurator UseFluentAnnotations(this IServiceCollection services)
    {
        var configurator = new ValidationConfigurator(services);
        services.Configure<ValidationBehaviorOptions>(_ => { }); // Default fallback
        return configurator;
    }
}
