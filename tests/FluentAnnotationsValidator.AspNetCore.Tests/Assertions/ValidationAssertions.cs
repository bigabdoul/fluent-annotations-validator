namespace FluentAnnotationsValidator.AspNetCore.Tests.Assertions;

public static class ValidationAssertions
{
    public static void ShouldMatch(this ConditionalValidationRule rule,
        string? expectedMessage = null,
        string? expectedKey = null,
        string? expectedResource = null,
        Func<object, bool>? expectedPredicate = null)
    {
        if (expectedMessage is not null)
            Assert.Equal(expectedMessage, rule.Message);

        if (expectedKey is not null)
            Assert.Equal(expectedKey, rule.Key);

        if (expectedResource is not null)
            Assert.Equal(expectedResource, rule.ResourceKey);

        if (expectedPredicate is not null)
        {
            Assert.NotNull(rule.Predicate.Target);
            Assert.True(expectedPredicate(rule.Predicate.Target));
        }
    }

    public static void ShouldNotMatch(this ConditionalValidationRule rule,
        string? expectedMessage = null,
        string? expectedKey = null,
        string? expectedResource = null,
        Func<object, bool>? expectedPredicate = null)
    {
        if (expectedMessage is not null)
            Assert.NotEqual(expectedMessage, rule.Message);

        if (expectedKey is not null)
            Assert.NotEqual(expectedKey, rule.Key);

        if (expectedResource is not null)
            Assert.NotEqual(expectedResource, rule.ResourceKey);

        if (expectedPredicate is not null)
        {
            Assert.NotNull(rule.Predicate.Target);
            Assert.False(expectedPredicate(rule.Predicate.Target));
        }
    }
}
