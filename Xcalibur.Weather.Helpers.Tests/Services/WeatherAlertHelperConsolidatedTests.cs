using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;

namespace Xcalibur.Weather.Helpers.Tests.Services;

/// <summary>
/// Tests for WeatherAlertHelper consolidated alert methods.
/// These tests verify the convenience methods that automatically consolidate alerts.
/// </summary>
public sealed class WeatherAlertHelperConsolidatedTests
{
    [Fact]
    public async Task BuildCombinedAlertsConsolidatedAsync_WithInvalidLatitude_ReturnsEmptyList()
    {
        // Arrange
        const string invalidLat = "invalid";
        const string validLon = "-74.0060";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
            invalidLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildCombinedAlertsConsolidatedAsync_WithInvalidLongitude_ReturnsEmptyList()
    {
        // Arrange
        const string validLat = "40.7128";
        const string invalidLon = "not-a-number";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
            validLat, invalidLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildCombinedAlertsConsolidatedAsync_WithValidCoordinates_AcceptsProvinceCode()
    {
        // Arrange
        const string validLat = "43.65";
        const string validLon = "-79.38";
        const string provinceCode = "ON";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None, provinceCode: provinceCode);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildCombinedAlertsConsolidatedAsync_WithValidCoordinates_AcceptsStateCode()
    {
        // Arrange
        const string validLat = "-33.87";
        const string validLon = "151.21";
        const string stateCode = "NSW";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None, stateCode: stateCode);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildCombinedAlertsConsolidatedAsync_ReturnsNonNullList()
    {
        // Arrange
        const string validLat = "40.7128";
        const string validLon = "-74.0060";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Result is a list (may be empty if no alerts are active)
    }
}
