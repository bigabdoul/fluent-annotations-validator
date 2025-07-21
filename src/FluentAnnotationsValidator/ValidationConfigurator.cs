using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator;

public class ValidationConfigurator : IValidationConfigurator
{
    private readonly IServiceCollection _services;
    private readonly List<Action<ValidationBehaviorOptions>> _registrations = [];

    public ValidationConfigurator(IServiceCollection services)
    {
        _services = services;
        _services.Configure<ValidationBehaviorOptions>(_ => { }); // Default fallback
    }

    public ValidationTypeConfigurator<T> For<T>()
        => new(this, typeof(T));

    public void Register(Action<ValidationBehaviorOptions> config)
        => _registrations.Add(config);

    public void Build()
    {
        foreach (var config in _registrations)
            _services.Configure(config);
    }

    IValidationTypeConfigurator<T> IValidationConfigurator.For<T>()
        => For<T>();
}

