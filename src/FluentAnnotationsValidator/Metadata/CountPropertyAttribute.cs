namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Indicates that a property should be used as the source of count-based validation
/// for custom types that do not implement standard collection interfaces.
/// <para>
/// This is useful when applying length-based validation to types that expose a count
/// through a custom property rather than implementing <see cref="ICollection"/> or <see cref="IEnumerable"/>.
/// </para>
/// <example>
/// public class CustomBatch
/// {
///     [CountProperty]
///     public int ItemCount { get; set; }
///
///     public string[] Items { get; set; }
/// }
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CountPropertyAttribute : Attribute
{
}