using FluentAnnotationsValidator.Runtime.Helpers;
using FluentAnnotationsValidator.Runtime.Validators;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FluentAnnotationsValidator.Internals.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class LengthCountAttribute : FluentValidationAttribute
{
    public int Minimum { get; }
    public int Maximum { get; }

    public LengthCountAttribute(int maximum)
        : base("The field '{0}' must not be longer than {1} characters.")
    {
        Maximum = maximum;
        EnsureLegalLengths();
    }

    public LengthCountAttribute(int minimum, int maximum) : base
    (
        minimum != maximum
        ? "The field '{0}' must be between {1} and {2} characters long."
        : "The field '{0}' must be exactly {1} characters long."
    )
    {
        Minimum = minimum;
        Maximum = maximum;
        EnsureLegalLengths();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (!CountHelper.TryGetCount(value, out int length))
            return ValidationResult.Success; // Length not applicable to this type

        return length < Minimum || length > Maximum && Maximum != -1
            ? GetFailedValidationResult(value, validationContext)
            : ValidationResult.Success;
    }

    /// <inheritdoc cref="FormatErrorMessage(string)"/>
    public override string FormatErrorMessage(string name)
    {
        EnsureLegalLengths();
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum);
    }

    /// <summary>
    /// Checks that <see cref="Minimum"/> and <see cref="Maximum"/> have legal values.
    /// Throws <see cref="ArgumentOutOfRangeException"/> if not.
    /// </summary>
    private void EnsureLegalLengths()
    {
        if (Minimum < 0) throw new ArgumentOutOfRangeException(nameof(Minimum), "Cannot be negative.");

        if (Maximum != -1 && Maximum < Minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(Maximum),
                $"Maximum should be larger than minimum.");
        }
    }
}
