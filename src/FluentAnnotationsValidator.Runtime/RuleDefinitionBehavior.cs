namespace FluentAnnotationsValidator.Runtime;

/// <summary>
/// Defines the behavior of rule definitions in the validation configuration.
/// </summary>
public enum RuleDefinitionBehavior
{
    /// <summary>
    /// Replaces existing rule definitions with new ones, discarding any previous rules.
    /// </summary>
    Replace,

    /// <summary>
    /// Preserves the existing rule definitions and adds new ones without replacing them.
    /// </summary>
    Preserve,
}