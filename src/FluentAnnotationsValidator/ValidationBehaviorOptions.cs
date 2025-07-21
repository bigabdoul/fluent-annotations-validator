namespace FluentAnnotationsValidator;

/// <summary>
/// Options for validation behavior (i.e., conditional validation).
/// </summary>
public class ValidationBehaviorOptions
{
    private readonly Dictionary<(Type modelType, string propertyName), ConditionalValidationRule> PropertyConditions = [];

    public virtual void Set(Type modelType, string propertyName, ConditionalValidationRule rule)
    {
        PropertyConditions[(modelType, propertyName)] = rule;
    }

    public virtual ConditionalValidationRule? Get(Type modelType, string propertyName)
    {
        if (PropertyConditions.TryGetValue((modelType, propertyName), out var predicate))
            return predicate;
        return null;
    }
}
