namespace FluentAnnotationsValidator;

public record ConditionalValidationRule(
    Func<object, bool> Predicate,
    string? Message = null,
    string? Key = null,
    string? ResourceKey = null);
