using FluentAssertions;

namespace Xcalibur.Weather.Helpers.Tests;

public sealed class TimeZoneHelperTests
{
    #region ConvertFromTimezone (DateTime? overload)

    [Fact]
    public void ConvertFromTimezone_NullableOverload_WithNullDateTime_ReturnsDateTimeMinValue()
    {
        DateTime? value = null;

        var result = value.ConvertFromTimezone("UTC");

        result.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void ConvertFromTimezone_NullableOverload_WithNullTimezone_ReturnsOriginalValue()
    {
        DateTime? value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezone(null);

        result.Should().Be(value.Value);
    }

    [Fact]
    public void ConvertFromTimezone_NullableOverload_WithNullDateTimeAndNullTimezone_ReturnsDateTimeMinValue()
    {
        DateTime? value = null;

        var result = value.ConvertFromTimezone(null);

        result.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void ConvertFromTimezone_NullableOverload_WithValidTimezone_ConvertsCorrectly()
    {
        // 2024-06-15 12:00 UTC is 08:00 Eastern (UTC-4 during DST)
        DateTime? value = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var result = value.ConvertFromTimezone("Eastern Standard Time");

        result.Hour.Should().Be(8);
    }

    #endregion

    #region ConvertFromTimezone (DateTime overload)

    [Fact]
    public void ConvertFromTimezone_WithNullTimezone_ReturnsOriginalDateTime()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezone(null);

        result.Should().Be(value);
    }

    [Fact]
    public void ConvertFromTimezone_WithUtcTimezone_ReturnsSameDateTime()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezone("UTC");

        result.Should().Be(value);
    }

    [Fact]
    public void ConvertFromTimezone_WithNonUtcTimezone_AppliesOffset()
    {
        // 2024-01-15 12:00 UTC is 05:00 Mountain Standard Time (UTC-7 in winter)
        var value = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        var result = value.ConvertFromTimezone("Mountain Standard Time");

        result.Hour.Should().Be(5);
    }

    #endregion

    #region ConvertFromTimezoneUtc (DateTime? overload)

    [Fact]
    public void ConvertFromTimezoneUtc_NullableOverload_WithNullDateTime_ReturnsDateTimeMinValue()
    {
        DateTime? value = null;

        var result = value.ConvertFromTimezoneUtc("UTC");

        result.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void ConvertFromTimezoneUtc_NullableOverload_WithNullTimezone_ReturnsOriginalValue()
    {
        DateTime? value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezoneUtc(null);

        result.Should().Be(value.Value);
    }

    [Fact]
    public void ConvertFromTimezoneUtc_NullableOverload_WithNullDateTimeAndNullTimezone_ReturnsDateTimeMinValue()
    {
        DateTime? value = null;

        var result = value.ConvertFromTimezoneUtc(null);

        result.Should().Be(DateTime.MinValue);
    }

    #endregion

    #region ConvertFromTimezoneUtc (DateTime overload)

    [Fact]
    public void ConvertFromTimezoneUtc_WithUtcTimezone_ReturnsSameDateTime()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezoneUtc("UTC");

        result.Should().Be(value);
    }

    [Fact]
    public void ConvertFromTimezoneUtc_WithNullTimezone_ReturnsOriginalDateTime()
    {
        var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        var result = value.ConvertFromTimezoneUtc(null);

        result.Should().Be(value);
    }

    [Fact]
    public void ConvertFromTimezoneUtc_WithNonUtcTimezone_AppliesOffset()
    {
        // 12:00 UTC interpreted as Eastern Standard Time (UTC-5 in winter) = 07:00 local
        var value = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        var result = value.ConvertFromTimezoneUtc("Eastern Standard Time");

        result.Hour.Should().Be(7);
    }

    #endregion
}