namespace FluentAnnotationsValidator.Tests.Models;

public class MinMaxTestModel<T> : IFluentValidatable
{
    public T Value { get; set; } = default!;
}
