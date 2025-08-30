namespace FluentAnnotationsValidator.Metadata;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ExactLengthAttribute(int length) : Length2Attribute(length, length)
{
}
