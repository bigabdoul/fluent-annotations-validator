namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that a string, collection, or other countable type must have an exact, specified length.
/// </summary>
/// <remarks>
/// This attribute is a convenience attribute that inherits from <see cref="Length2Attribute"/>
/// and sets both the minimum and maximum length to the same value. It is used to
/// enforce a precise character count for strings or item count for collections and arrays.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ExactLengthAttribute(int length) : Length2Attribute(length, length)
{
}