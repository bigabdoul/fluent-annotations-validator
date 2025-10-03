using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Annotations;

/// <summary>
/// Specifies the maximum allowable value for a data field. The minimum is automatically set to the minimum value of the corresponding type.
/// </summary>
public class MaximumAttribute : RangeAttribute
{
    private readonly bool _isObjectValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="byte"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(byte value) : base(byte.MinValue, value) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="short"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(short value) : base(short.MinValue, value) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="int"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(int value) : base(int.MinValue, value) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="long"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(long value) : base(typeof(long), $"{long.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="Int128"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MaximumAttribute(Int128 value) : base(typeof(Int128), $"{Int128.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="IntPtr"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MaximumAttribute(IntPtr value) : base(typeof(IntPtr), $"{IntPtr.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="float"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(float value) : base(typeof(float), $"{float.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="double"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(double value) : base(double.MinValue, value) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="decimal"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MaximumAttribute(decimal value) : base(typeof(decimal), $"{decimal.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="ushort"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(ushort value) : base(typeof(ushort), $"{ushort.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="uint"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(uint value) : base(typeof(uint), $"{uint.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="UIntPtr"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MaximumAttribute(UIntPtr value) : base(typeof(UIntPtr), $"{UIntPtr.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="ulong"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    public MaximumAttribute(ulong value) : base(typeof(ulong), $"{ulong.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="UInt128"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MaximumAttribute(UInt128 value) : base(typeof(UInt128), $"{UInt128.MinValue}", value.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaximumAttribute"/> class for <see cref="object"/> values.
    /// </summary>
    /// <param name="value">The maximum allowable value.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public MaximumAttribute(object value) : base(value.GetType(), AttributeUtils.GetMinimumString(value), $"{value}")
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

        IComparable max = (IComparable)(operandType.IsEnum
                ? Convert.ChangeType(converter.ConvertFromString((string)Maximum), Enum.GetUnderlyingType(operandType))
                : ConvertValueInInvariantCulture
                    ? converter.ConvertFromInvariantString((string)Maximum)!
                        : converter.ConvertFromString((string)Maximum))!;

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

        return MaximumIsExclusive ? max.CompareTo(convertedValue) > 0 : max.CompareTo(convertedValue) >= 0;
    }
}