namespace FluentAnnotationsValidator.Annotations;

internal static class AttributeUtils
{
    internal static string GetMinimumString(object value)
    {
        return value switch
        {
            decimal => decimal.MinValue.ToString(),
            IntPtr => IntPtr.MinValue.ToString(),
            UIntPtr => UIntPtr.MinValue.ToString(),
            Int128 => Int128.MinValue.ToString(),
            UInt128 => UInt128.MinValue.ToString(),
            _ => value.ToString() ?? string.Empty,
        };
    }

    internal static string GetMaximumString(object value)
    {
        return value switch
        {
            decimal => decimal.MaxValue.ToString(),
            IntPtr => IntPtr.MaxValue.ToString(),
            UIntPtr => UIntPtr.MaxValue.ToString(),
            Int128 => Int128.MaxValue.ToString(),
            UInt128 => UInt128.MaxValue.ToString(),
            _ => value.ToString() ?? string.Empty,
        };
    }
}
