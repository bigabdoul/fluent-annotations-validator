using FluentAnnotationsValidator.Abstractions;
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Runtime.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that a string, collection, or other countable type must have a length or count within a specified range.
/// </summary>
/// <remarks>
/// This attribute extends the built-in <see cref="LengthAttribute"/>
/// to provide more flexible length validation. It can be applied to strings, collections
/// (like <see cref="List{T}"/>), and arrays to check if their count falls within the
/// specified minimum and maximum bounds.
/// <para>
/// This attribute does not validate against <see langword="null"/> values. Use the
/// <see cref="RequiredAttribute"/> for that purpose.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class Length2Attribute : LengthAttribute
{
    /// <summary>
    /// Gets or sets an optional message resolver to provide localized error messages.
    /// </summary>
    public IValidationMessageResolver? MessageResolver { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Length2Attribute"/> class with a maximum length.
    /// </summary>
    /// <param name="maximum">The maximum allowed length or count.</param>
    public Length2Attribute(int maximum) : base(0, maximum)
    {
        EnsureLegalLengths();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Length2Attribute"/> class with a minimum and maximum length.
    /// </summary>
    /// <param name="minimum">The minimum allowed length or count.</param>
    /// <param name="maximum">The maximum allowed length or count.</param>
    public Length2Attribute(int minimum, int maximum) : base(minimum, maximum)
    {
        EnsureLegalLengths();
    }

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        EnsureLegalLengths();

        if (!CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success; // Length not applicable to this type

        return length < MinimumLength || (MaximumLength != -1 && length > MaximumLength)
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
    /// Checks that <see cref="LengthAttribute.MinimumLength"/> and <see cref="LengthAttribute.MaximumLength"/> have legal values.
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