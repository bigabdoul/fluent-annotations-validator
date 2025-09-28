using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies the minimum allowable value for a data field. The maximum is automatically set to the maximum value of the corresponding type.
/// </summary>
public class MinimumAttribute : RangeAttribute
{
    private readonly bool _isObjectValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="byte"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(byte value) : base(value, byte.MaxValue) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="short"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(short value) : base(value, short.MaxValue) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="int"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(int value) : base(value, int.MaxValue) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="long"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(long value) : base(typeof(long), value.ToString(), $"{long.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="Int128"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(Int128 value) : base(typeof(Int128), value.ToString(), $"{Int128.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="IntPtr"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(IntPtr value) : base(typeof(IntPtr), value.ToString(), $"{IntPtr.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="float"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(float value) : base(typeof(float), value.ToString(), $"{float.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="double"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(double value) : base(value, double.MaxValue) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="decimal"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(decimal value) : base(typeof(decimal), value.ToString(), $"{decimal.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="ushort"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(ushort value) : base(typeof(ushort), value.ToString(), $"{ushort.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="uint"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(uint value) : base(typeof(uint), value.ToString(), $"{uint.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="ulong"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(ulong value) : base(typeof(ulong), value.ToString(), $"{ulong.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="UInt128"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(UInt128 value) : base(typeof(UInt128), value.ToString(), $"{UInt128.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="UIntPtr"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(UIntPtr value) : base(typeof(UIntPtr), value.ToString(), $"{UIntPtr.MaxValue}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAttribute"/> class for <see cref="object"/> values.
    /// </summary>
    /// <param name="value">The minimum allowable value.</param>
    public MinimumAttribute(object value) : base(value.GetType(), $"{value}", $"{value}")
    {
        if (!typeof(IComparable).IsAssignableFrom(OperandType))
            throw new InvalidOperationException($"Arbitrary type {OperandType.Name} is not comparable.");
        _isObjectValue = true;
    }

    /// <inheritdoc/>
    public override bool IsValid(object? value)
    {
        if (!_isObjectValue)
            return base.IsValid(value);

        // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
        if (value is null or string { Length: 0 })
        {
            return true;
        }

        if (!typeof(IComparable).IsAssignableFrom(value.GetType()))
            return false;

        Type operandType = OperandType;
        TypeConverter converter = TypeDescriptor.GetConverter(operandType);

        IComparable min = (IComparable)(operandType.IsEnum
                ? Convert.ChangeType(converter.ConvertFromString((string)Minimum), Enum.GetUnderlyingType(operandType))
                : ConvertValueInInvariantCulture
                    ? converter.ConvertFromInvariantString((string)Minimum)!
                        : converter.ConvertFromString((string)Minimum))!;

        object? convertedValue;

        try
        {
            convertedValue = operandType.IsEnum
                ? Convert.ChangeType(value, Enum.GetUnderlyingType(operandType))
                : ConvertValueInInvariantCulture
                    ? converter.ConvertFrom(null, CultureInfo.InvariantCulture, value)
                    : converter.ConvertFrom(value);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (InvalidCastException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }

        return MinimumIsExclusive ? min.CompareTo(convertedValue) < 0 : min.CompareTo(convertedValue) <= 0;
    }
}