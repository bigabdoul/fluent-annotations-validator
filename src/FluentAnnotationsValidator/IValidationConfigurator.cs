namespace FluentAnnotationsValidator;

public interface IValidationConfigurator
{
    IValidationTypeConfigurator<T> For<T>();
    void Build(); // optional: finalize if needed
}
