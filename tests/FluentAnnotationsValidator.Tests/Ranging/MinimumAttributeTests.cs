using FluentAssertions;

namespace FluentAnnotationsValidator.Tests.Ranging;

public class MinimumAttributeTests : MinMaxAttributeTestsBase
{
    [Theory]
    [InlineData((byte)5, (byte)4, false)]
    [InlineData((byte)5, (byte)5, true)]
    [InlineData((byte)5, (byte)6, true)]
    public void ByteMinimumValidation(byte min, byte input, bool expectedValid)
    {
        // Arrange
        var validator = RuleFor<byte>(rule => rule.Minimum(min));

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(100, 99, false)]
    [InlineData(100, 100, true)]
    [InlineData(100, 101, true)]
    public void IntMinimumValidation(int min, int input, bool expectedValid)
    {
        // Arrange
        var validator = RuleFor<int>(rule => rule.Minimum(min));

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void DecimalMinimumValidation_ShouldFailBelowMinimum()
    {
        // Arrange
        var validator = RuleFor<decimal>(rule => rule.Minimum(10.5m));

        // Act
        var result = validator.Validate(9.9m);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ObjectMinimumValidation_ShouldFailBelowMinimum()
    {
        // Arrange
        var validator = RuleFor<object>(rule => rule.Minimum(DayOfWeek.Friday));

        // Act
        var result = validator.Validate(DayOfWeek.Thursday);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
