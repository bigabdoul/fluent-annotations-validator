using FluentAnnotationsValidator.Tests.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAnnotationsValidator.Tests.Ranging;

public abstract class MinMaxAttributeTestsBase
{
    #region fields

    protected readonly ServiceCollection Services = new();
    protected readonly ServiceProvider ServiceProvider;
    protected readonly FluentTypeValidatorRoot ValidatorRoot;

    #endregion

    #region constructor

    protected MinMaxAttributeTestsBase()
    {
        Services.AddFluentAnnotationsValidators(new ConfigurationOptions
        {
            TargetAssembliesTypes= [typeof(MinMaxAttributeTestsBase)],
            ExtraValidatableTypesFactory = () =>
            [
                typeof(MinMaxTestModel<byte>),
                typeof(MinMaxTestModel<short>),
                typeof(MinMaxTestModel<int>),
                typeof(MinMaxTestModel<long>),
                typeof(MinMaxTestModel<Int128>),
                typeof(MinMaxTestModel<IntPtr>),
                typeof(MinMaxTestModel<float>),
                typeof(MinMaxTestModel<double>),
                typeof(MinMaxTestModel<decimal>),
                typeof(MinMaxTestModel<ushort>),
                typeof(MinMaxTestModel<uint>),
                typeof(MinMaxTestModel<ulong>),
                typeof(MinMaxTestModel<UInt128>),
                typeof(MinMaxTestModel<UIntPtr>),
                typeof(MinMaxTestModel<DayOfWeek>),

                // Nullable versions
                typeof(MinMaxTestModel<byte?>),
                typeof(MinMaxTestModel<short?>),
                typeof(MinMaxTestModel<int?>),
                typeof(MinMaxTestModel<long?>),
                typeof(MinMaxTestModel<Int128?>),
                typeof(MinMaxTestModel<IntPtr?>),
                typeof(MinMaxTestModel<float?>),
                typeof(MinMaxTestModel<double?>),
                typeof(MinMaxTestModel<decimal?>),
                typeof(MinMaxTestModel<ushort?>),
                typeof(MinMaxTestModel<uint?>),
                typeof(MinMaxTestModel<ulong?>),
                typeof(MinMaxTestModel<UInt128?>),
                typeof(MinMaxTestModel<UIntPtr?>),
                typeof(MinMaxTestModel<DayOfWeek?>),

                typeof(MinMaxTestModel<object>),
            ],
        });
        ServiceProvider = Services.BuildServiceProvider();
        ValidatorRoot = ServiceProvider.GetRequiredService<FluentTypeValidatorRoot>();
    }

    #endregion

    protected IFluentValidator<MinMaxTestModel<T>> RuleFor<T>(Action<IValidationRuleBuilder<MinMaxTestModel<T>, T>> configure)
    {
        var config = ValidatorRoot.For<MinMaxTestModel<T>>();
        configure(config.RuleFor(x => x.Value));
        config.Build();
        return ServiceProvider.GetRequiredService<IFluentValidator<MinMaxTestModel<T>>>();
    }
}
