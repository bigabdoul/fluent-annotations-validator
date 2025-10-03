using System.Globalization;

namespace FluentAnnotationsValidator.Annotations;

/// <summary>
/// Specifies that a string, collection, or other countable type must have an exact, specified length.
/// </summary>
/// <remarks>
/// This attribute is a convenience attribute that inherits from <see cref="LengthCountAttribute"/>
/// and sets both the minimum and maximum length to the same value. It is used to
/// enforce a precise character count for strings or item count for collections and arrays.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ExactLengthAttribute(int length) : LengthCountAttribute(length, length, "The string or collection {0} must contain exactly {1} items.")
{
    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
    {
        EnsureLegalLengths();
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MinimumLength);
    }

    /// <inheritdoc />
    protected override void EnsureLegalLengths()
    {
        base.EnsureLegalLengths();
        if (MinimumLength != MaximumLength)
            throw new ArgumentException("The lengths are not matching.");
    }
}