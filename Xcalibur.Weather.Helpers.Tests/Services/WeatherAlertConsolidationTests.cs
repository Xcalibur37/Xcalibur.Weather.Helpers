using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models.Implementation.WeatherAlerts;

namespace Xcalibur.Weather.Helpers.Tests.Services;

/// <summary>
/// Tests for WeatherAlertHelper alert consolidation functionality.
/// </summary>
public sealed class WeatherAlertConsolidationTests
{
    private const string WinterStormType = "WinterStorm";
    private const string TornadoType = "Tornado";
    private const string FloodType = "Flood";
    private const string FlashFloodType = "FlashFlood";
    private const string SevereThunderstormType = "SevereThunderstorm";
    private const string HeatType = "Heat";
    private const string HurricaneType = "Hurricane";

    [Fact]
    public void ConsolidateAlerts_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(null!, NullLogger.Instance);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ConsolidateAlerts_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var alerts = new List<WeatherAlertItem>();

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ConsolidateAlerts_WithSingleAlert_ReturnsSameAlert()
    {
        // Arrange
        var alert = CreateAlert("Winter Storm Warning", AlertSeverity.Severe, WinterStormType, DateTime.UtcNow);
        var alerts = new List<WeatherAlertItem> { alert };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeSameAs(alert);
    }

    [Fact]
    public void ConsolidateAlerts_WithDifferentEventTypes_KeepsAllAlerts()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Winter Storm Warning", AlertSeverity.Severe, WinterStormType, now),
            CreateAlert("Tornado Watch", AlertSeverity.Extreme, TornadoType, now),
            CreateAlert("Flood Advisory", AlertSeverity.Minor, FloodType, now)
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void ConsolidateAlerts_WithOverlappingAlertsOfSameType_KeepsHighestSeverity()
    {
        // Arrange - All winter weather alerts with overlapping time periods
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Winter Weather Advisory", AlertSeverity.Minor, WinterStormType, now, now.AddHours(6)),
            CreateAlert("Winter Storm Watch", AlertSeverity.Moderate, WinterStormType, now.AddHours(1), now.AddHours(8)),
            CreateAlert("Winter Storm Warning", AlertSeverity.Severe, WinterStormType, now.AddHours(2), now.AddHours(10))
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(1);
        result[0].EventType.Should().Be(WinterStormType);
        result[0].Severity.Should().Be(AlertSeverity.Severe);
    }

    [Fact]
    public void ConsolidateAlerts_WithNonOverlappingAlertsOfSameType_KeepsAllAlerts()
    {
        // Arrange - Same event type but non-overlapping time periods
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Severe Thunderstorm Warning", AlertSeverity.Severe, SevereThunderstormType, now, now.AddHours(2)),
            CreateAlert("Severe Thunderstorm Watch", AlertSeverity.Moderate, SevereThunderstormType, now.AddHours(3), now.AddHours(5))
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void ConsolidateAlerts_WithEqualSeverity_KeepsMostRecent()
    {
        // Arrange - Two alerts with same severity, one is more recent
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Flood Warning", AlertSeverity.Severe, FloodType, now, now.AddHours(6)),
            CreateAlert("Flood Warning (Updated)", AlertSeverity.Severe, FloodType, now.AddHours(1), now.AddHours(7))
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(1);
        result[0].EventType.Should().Be(FloodType);
        result[0].Effective.Should().Be(now.AddHours(1));
    }

    [Fact]
    public void ConsolidateAlerts_WithMixedScenarios_ConsolidatesCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            // Group 1: Overlapping winter weather (should consolidate to 1)
            CreateAlert("Winter Weather Advisory", AlertSeverity.Minor, WinterStormType, now, now.AddHours(6)),
            CreateAlert("Winter Storm Warning", AlertSeverity.Severe, WinterStormType, now.AddHours(1), now.AddHours(8)),

            // Group 2: Separate tornado alert (should keep)
            CreateAlert("Tornado Watch", AlertSeverity.Extreme, TornadoType, now, now.AddHours(4)),

            // Group 3: Non-overlapping flood alerts (should keep both)
            CreateAlert("Flood Warning", AlertSeverity.Severe, FloodType, now, now.AddHours(2)),
            CreateAlert("Flash Flood Watch", AlertSeverity.Moderate, FlashFloodType, now.AddHours(3), now.AddHours(5))
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(4); // Winter (1) + Tornado (1) + Flood (1) + Flash Flood (1)
        result.Should().Contain(a => a.EventType == WinterStormType);
        result.Should().Contain(a => a.EventType == TornadoType);
        result.Should().Contain(a => a.EventType == FloodType);
        result.Should().Contain(a => a.EventType == FlashFloodType);
    }

    [Fact]
    public void ConsolidateAlerts_WithAlertsWithoutEffectiveTime_DoesNotConsolidate()
    {
        // Arrange - Alerts without time information should not be consolidated
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Heat Advisory", AlertSeverity.Minor, HeatType, null, null),
            CreateAlert("Excessive Heat Warning", AlertSeverity.Severe, HeatType, null, null)
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(2); // Should keep both due to missing time info
    }

    [Fact]
    public void ConsolidateAlerts_WithPartialTimeOverlap_Consolidates()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Alert 1", AlertSeverity.Minor, WinterStormType, now, now.AddHours(4)),
            CreateAlert("Alert 2", AlertSeverity.Severe, WinterStormType, now.AddHours(3), now.AddHours(7)) // Overlaps by 1 hour
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(1);
        result[0].Severity.Should().Be(AlertSeverity.Severe);
    }

    [Fact]
    public void ConsolidateAlerts_WithExtremeVsMinorSeverity_KeepsExtreme()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var alerts = new List<WeatherAlertItem>
        {
            CreateAlert("Hurricane Watch", AlertSeverity.Minor, HurricaneType, now, now.AddHours(12)),
            CreateAlert("Hurricane Warning", AlertSeverity.Extreme, HurricaneType, now.AddHours(1), now.AddHours(10))
        };

        // Act
        var result = WeatherAlertHelper.ConsolidateAlerts(alerts, NullLogger.Instance);

        // Assert
        result.Should().HaveCount(1);
        result[0].EventType.Should().Be(HurricaneType);
        result[0].Severity.Should().Be(AlertSeverity.Extreme);
    }

    #region Helper Methods

    private static WeatherAlertItem CreateAlert(
        string eventName,
        AlertSeverity severity,
        string eventType,
        DateTime? effective = null,
        DateTime? expires = null)
    {
        return new WeatherAlertItem
        {
            Event = AlertEventType.Unknown,
            Severity = severity,
            EventType = eventType,
            Effective = effective, // Don't default to UtcNow - use the provided value
            Expires = expires,
            Source = "NWS",
            Headline = eventName,
            Description = $"Test alert for {eventName}"
        };
    }

    #endregion
}
