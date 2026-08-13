using FluentAssertions;

namespace Xcalibur.Weather.Helpers.Tests;

public sealed class TimeZoneHelperTests
{
    [Fact]
    public void ConvertFromTimezone_WithNullDateTime_ReturnsDateTimeMinValue()
    {
        DateTime? value = null;

        var result = value.ConvertFromTimezone("UTC");

        result.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void ConvertFromTimezone_WithNullTimezone_ReturnsOriginalDateTime()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezone(null);

        result.Should().Be(value);
    }

    [Fact]
    public void ConvertFromTimezoneUtc_WithUtcTimezone_ReturnsSameDateTime()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezoneUtc("UTC");

        result.Should().Be(value);
    }
}