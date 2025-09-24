namespace FluentAnnotationsValidator.Metadata;

/// <summary>
/// Specifies that validation rules should be inherited from another class.
/// The attributes from the source class will be applied to matching properties
/// on the target class.
/// </summary>
/// <remarks>
/// This attribute simplifies validation configuration by centralizing rules
/// in a single class and applying them to multiple models.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="InheritRulesAttribute"/> class.
/// </remarks>
/// <param name="objectType">The type from which to inherit validation attributes.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InheritRulesAttribute(Type objectType) : FluentRuleAttribute(objectType)
{
}