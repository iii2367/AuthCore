using AuthCore.Domain.Common;

namespace AuthCore.Domain.Tests.Common;

public class DateTimeOffsetValidatorTests
{
    [Fact]
    public void ValidateIsSet_WhenValueIsValid_DoesNotThrow()
    {
        DateTimeOffset value = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

        DateTimeOffsetValidator.ValidateIsSet(value, nameof(value));
    }

    [Fact]
    public void ValidateIsSet_WhenValueIsMinValue_ThrowsDomainException()
    {
        DateTimeOffset value = DateTimeOffset.MinValue;

        Assert.Throws<DomainException>(() => DateTimeOffsetValidator.ValidateIsSet(value, nameof(value)));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(-2)]
    public void ValidateHasNonZeroOffset_WhenValueIsValid_DoesNotThrow(double offsetHours)
    {
        DateTimeOffset value = new(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(offsetHours));

        DateTimeOffsetValidator.ValidateHasNonZeroOffset(value, nameof(value));
    }

    [Fact]
    public void ValidateHasNonZeroOffset_WhenOffsetIsZero_ThrowsDomainException()
    {
         DateTimeOffset value = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainException>(() => DateTimeOffsetValidator.ValidateHasNonZeroOffset(value, nameof(value)));
    }

    [Fact]
    public void ValidateIsUtc_WhenOffsetIsZero_DoesNotThrow()
    {
        DateTimeOffset value = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    
        DateTimeOffsetValidator.ValidateIsUtc(value, nameof(value));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(-2)]
    public void ValidateIsUtc_WhenOffsetIsNonZero_ThrowsDomainException(double offsetHours)
    {
        DateTimeOffset value = new(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(offsetHours));

        Assert.Throws<DomainException>(() => DateTimeOffsetValidator.ValidateIsUtc(value, nameof(value))); 
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void ValidateNotEarlier_WhenValueIsValid_DoesNotThrow(int hours)
    {
        DateTimeOffset previous = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset value = previous.AddHours(hours);

        DateTimeOffsetValidator.ValidateNotEarlier(
        value, previous, nameof(value), nameof(previous));
    } 

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void ValidateNotEarlier_WhenValueIsEarlier_ThrowsDomainException(int hours)
    {
        DateTimeOffset previous = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset value = previous.AddHours(hours);

        Assert.Throws<DomainException>(() => DateTimeOffsetValidator.ValidateNotEarlier(value, previous, nameof(value), nameof(previous)));
    } 

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void ValidateLater_WhenValueIsValid_DoesNotThrow(int hours)
    {
        DateTimeOffset previous = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset value = previous.AddHours(hours);

        DateTimeOffsetValidator.ValidateLater(value, previous, nameof(value), nameof(previous));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void ValidateLater_WhenValueIsNotLater_ThrowsDomainException(int hours)
    {
        DateTimeOffset previous = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset value = previous.AddHours(hours);

        Assert.Throws<DomainException>(() => DateTimeOffsetValidator.ValidateLater(value, previous, nameof(value), nameof(previous)));
    } 
}
