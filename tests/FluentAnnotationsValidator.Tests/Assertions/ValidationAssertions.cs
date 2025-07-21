using FluentAnnotationsValidator.Configuration;

namespace FluentAnnotationsValidator.Tests.Assertions;

public static class ValidationAssertions
{
    public static void ShouldMatch(this ConditionalValidationRule rule,
        string? expectedMessage = null,
        string? expectedKey = null,
        string? expectedResource = null,
        object? predicateArg = null)
    {
        if (expectedMessage is not null)
            Assert.Equal(expectedMessage, rule.Message);

        if (expectedKey is not null)
            Assert.Equal(expectedKey, rule.Key);

        if (expectedResource is not null)
            Assert.Equal(expectedResource, rule.ResourceKey);

        if (predicateArg is not null)
        {
            Assert.NotNull(rule.Predicate.Target);
            Assert.True(rule.Predicate(predicateArg));
        }
    }

    public static void ShouldNotMatch(this ConditionalValidationRule rule,
        string? expectedMessage = null,
        string? expectedKey = null,
        string? expectedResource = null,
        object? predicateArg = null)
    {
        if (expectedMessage is not null)
            Assert.NotEqual(expectedMessage, rule.Message);

        if (expectedKey is not null)
            Assert.NotEqual(expectedKey, rule.Key);

        if (expectedResource is not null)
            Assert.NotEqual(expectedResource, rule.ResourceKey);

        if (predicateArg is not null)
        {
            Assert.NotNull(rule.Predicate.Target);
            Assert.False(rule.Predicate(predicateArg));
        }
    }
}
