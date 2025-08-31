using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class Length2Attribute : LengthAttribute
{
    public IValidationMessageResolver? MessageResolver { get; set; }

    public Length2Attribute(int maximum) : base(0, maximum)
    {
        EnsureLegalLengths();
    }

    public Length2Attribute(int minimum, int maximum) : base(minimum, maximum)
    {
        EnsureLegalLengths();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        EnsureLegalLengths();

        if (!CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success; // Length not applicable to this type

        return length < MinimumLength || length > MaximumLength && MaximumLength != -1
            ? this.GetFailedValidationResult(validationContext, MessageResolver)
            : ValidationResult.Success;
    }

    /// <inheritdoc cref="FormatErrorMessage(string)"/>
    public override string FormatErrorMessage(string name)
    {
        EnsureLegalLengths();
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MinimumLength, MaximumLength);
    }

    /// <summary>
    /// Checks that <see cref="MinimumLength"/> and <see cref="MaximumLength"/> have legal values.
    /// Throws <see cref="ArgumentOutOfRangeException"/> if not.
    /// </summary>
    private void EnsureLegalLengths()
    {
        if (MinimumLength < 0) throw new ArgumentOutOfRangeException(nameof(MinimumLength), "Cannot be negative.");

        if (MaximumLength != -1 && MaximumLength < MinimumLength)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumLength),
                $"Maximum should be larger than minimum.");
        }
    }
}
