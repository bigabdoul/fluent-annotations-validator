namespace FluentAnnotationsValidator.Tests.Ranging;

using Models;

internal static class RangeAttributeExtensions
{
    internal static FluentValidationResult Validate<T>(this IFluentValidator<MinMaxTestModel<T>> validator, T value)
        => validator.Validate(new MinMaxTestModel<T> { Value = value });
}