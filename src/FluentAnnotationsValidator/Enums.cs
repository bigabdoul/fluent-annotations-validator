namespace FluentAnnotationsValidator;


/// <summary>
/// Specifies how rules should cascade when one fails.
/// </summary>
public enum CascadeMode
{
    /// <summary>
    /// When a rule/validator fails, execution continues to the next rule/validator.
    /// For more information, see the methods/properties that accept this enum as a parameter.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// When a rule/validator fails, validation is stopped for the current rule/validator.
    /// For more information, see the methods/properties that accept this enum as a parameter.
    /// </summary>
    Stop = 2, // Note Stop is 2 for backwards compatibility. The StopOnFirstFailure option was 1, which was removed in 12.0. This is explicitly set to 2 to prevent it from being automatically renumbered to 1.
}

/// <summary>
/// Specifies where a When/Unless condition should be applied
/// </summary>
public enum ApplyConditionTo
{
    /// <summary>
    /// Applies the condition to all validators declared so far in the chain.
    /// </summary>
    AllValidators,
    /// <summary>
    /// Applies the condition to the current validator only.
    /// </summary>
    CurrentValidator
}
