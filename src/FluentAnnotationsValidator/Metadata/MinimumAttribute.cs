using System.ComponentModel.DataAnnotations;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies the minimum allowable value for a data field. The maximum is automatically set to the maximum value of the corresponding type.
/// </summary>
public class MinimumAttribute : RangeAttribute
{
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
}