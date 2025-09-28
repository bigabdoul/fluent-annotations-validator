using FluentAssertions;

namespace FluentAnnotationsValidator.Tests.Ranging;

public class FluentChainingTests : MinMaxAttributeTestsBase
{
    [Theory]
    [InlineData(15, true)]
    [InlineData(9, false)]
    [InlineData(21, false)]
    public void MinimumAndMaximumChaining_ShouldValidateRange(int value, bool expectedValid)
    {
        // Arrange
        var validator = RuleFor<int>(rule => rule.Minimum(10).Maximum(20));

        // Act
        var result = validator.Validate(value);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void NullValue_ShouldBeValidUnlessRequired()
    {
        // Arrange
        var validator = RuleFor<int?>(rule => rule.Minimum(10).Maximum(20));
       
        // Act
        var result = validator.Validate(new int?());

        // Assert
        result.IsValid.Should().BeTrue(); // Range doesn't apply to null
    }
}
