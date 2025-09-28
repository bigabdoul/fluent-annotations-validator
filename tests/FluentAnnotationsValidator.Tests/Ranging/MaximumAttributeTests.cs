using FluentAssertions;

namespace FluentAnnotationsValidator.Tests.Ranging;

public class MaximumAttributeTests : MinMaxAttributeTestsBase
{
    [Theory]
    [InlineData((byte)10, (byte)11, false)]
    [InlineData((byte)10, (byte)10, true)]
    [InlineData((byte)10, (byte)9, true)]
    public void ByteMaximumValidation(byte max, byte input, bool expectedValid)
    {
        // Arrange
        var validator = RuleFor<byte>(rule => rule.Maximum(max));

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(100, 101, false)]
    [InlineData(100, 100, true)]
    [InlineData(100, 99, true)]
    public void IntMaximumValidation(int max, int input, bool expectedValid)
    {
        // Arrange
        var validator = RuleFor<int>(rule => rule.Maximum(max));

        // Act
        var result = validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void DecimalMaximumValidation_ShouldFailAboveMaximum()
    {
        // Arrange
        var validator = RuleFor<decimal>(rule => rule.Maximum(10.5m));

        // Act
        var result = validator.Validate(11.1m);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ObjectMaximumValidation_ShouldFailAboveMaximum()
    {
        // Arrange
        var validator = RuleFor<DayOfWeek>(rule => rule.Maximum(DayOfWeek.Friday));

        // Act
        var result = validator.Validate(DayOfWeek.Saturday);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
