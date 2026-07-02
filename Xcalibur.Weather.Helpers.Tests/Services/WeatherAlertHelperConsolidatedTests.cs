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
    public async Task BuildNwsAlertsConsolidatedAsync_WithInvalidLatitude_ReturnsEmptyList()
    {
        // Arrange
        const string invalidLat = "invalid";
        const string validLon = "-74.0060";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildNwsAlertsConsolidatedAsync(
            invalidLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildNwsAlertsConsolidatedAsync_WithValidCoordinates_DoesNotThrow()
    {
        // Arrange
        const string validLat = "40.7128";
        const string validLon = "-74.0060";
        var logger = NullLogger.Instance;

        // Act & Assert - Should not throw
        var result = await WeatherAlertHelper.BuildNwsAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None);

        // Result should be a valid list (may be empty if no alerts)
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

    [Fact]
    public async Task BuildNwsAlertsConsolidatedAsync_ReturnsNonNullList()
    {
        // Arrange
        const string validLat = "40.7128";
        const string validLon = "-74.0060";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildNwsAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Result is a list (may be empty if no alerts are active)
    }

    #region Meteoalarm Consolidated Tests

    [Fact]
    public async Task BuildMeteoalarmAlertsConsolidatedAsync_WithValidCoordinates_ReturnsNonNullList()
    {
        // Arrange - European coordinates (Rome, Italy)
        const string validLat = "41.9028";
        const string validLon = "12.4964";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildMeteoalarmAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildMeteoalarmAlertsConsolidatedAsync_WithInvalidCoordinates_ReturnsEmptyList()
    {
        // Arrange
        const string invalidLat = "invalid";
        const string validLon = "12.4964";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildMeteoalarmAlertsConsolidatedAsync(
            invalidLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region Environment Canada Consolidated Tests

    [Fact]
    public async Task BuildEnvironmentCanadaAlertsConsolidatedAsync_WithValidParameters_ReturnsNonNullList()
    {
        // Arrange - Toronto, Ontario coordinates
        const string validLat = "43.6532";
        const string validLon = "-79.3832";
        const string provinceCode = "on";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildEnvironmentCanadaAlertsConsolidatedAsync(
            validLat, validLon, provinceCode, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildEnvironmentCanadaAlertsConsolidatedAsync_WithInvalidCoordinates_ReturnsEmptyList()
    {
        // Arrange
        const string invalidLat = "not-a-number";
        const string validLon = "-79.3832";
        const string provinceCode = "on";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildEnvironmentCanadaAlertsConsolidatedAsync(
            invalidLat, validLon, provinceCode, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildEnvironmentCanadaAlertsConsolidatedAsync_WithDifferentProvinces_AcceptsParameter()
    {
        // Arrange - Vancouver, BC coordinates
        const string validLat = "49.2827";
        const string validLon = "-123.1207";
        const string provinceCode = "bc";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildEnvironmentCanadaAlertsConsolidatedAsync(
            validLat, validLon, provinceCode, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region BOM Consolidated Tests

    [Fact]
    public async Task BuildBomAlertsConsolidatedAsync_WithValidParameters_ReturnsNonNullList()
    {
        // Arrange - Sydney, NSW coordinates
        const string validLat = "-33.8688";
        const string validLon = "151.2093";
        const string stateCode = "nsw";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildBomAlertsConsolidatedAsync(
            validLat, validLon, stateCode, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildBomAlertsConsolidatedAsync_WithInvalidCoordinates_ReturnsEmptyList()
    {
        // Arrange
        const string validLat = "-33.8688";
        const string invalidLon = "invalid-lon";
        const string stateCode = "nsw";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildBomAlertsConsolidatedAsync(
            validLat, invalidLon, stateCode, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildBomAlertsConsolidatedAsync_WithDifferentStates_AcceptsParameter()
    {
        // Arrange - Melbourne, VIC coordinates
        const string validLat = "-37.8136";
        const string validLon = "144.9631";
        const string stateCode = "vic";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildBomAlertsConsolidatedAsync(
            validLat, validLon, stateCode, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GDACS Consolidated Tests

    [Fact]
    public async Task BuildGdacsAlertsConsolidatedAsync_WithValidCoordinates_ReturnsNonNullList()
    {
        // Arrange - Global coordinates (Tokyo, Japan)
        const string validLat = "35.6762";
        const string validLon = "139.6503";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildGdacsAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildGdacsAlertsConsolidatedAsync_WithInvalidCoordinates_ReturnsEmptyList()
    {
        // Arrange
        const string invalidLat = "not-valid";
        const string validLon = "139.6503";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildGdacsAlertsConsolidatedAsync(
            invalidLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region DWD Consolidated Tests

    [Fact]
    public async Task BuildDwdAlertsConsolidatedAsync_WithValidCoordinates_ReturnsNonNullList()
    {
        // Arrange - German coordinates (Berlin)
        const string validLat = "52.52";
        const string validLon = "13.41";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildDwdAlertsConsolidatedAsync(
            validLat, validLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildDwdAlertsConsolidatedAsync_WithInvalidCoordinates_ReturnsEmptyList()
    {
        // Arrange
        const string validLat = "52.52";
        const string invalidLon = "invalid";
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildDwdAlertsConsolidatedAsync(
            validLat, invalidLon, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region EMSC Consolidated Tests

    [Fact]
    public async Task BuildEmscAlertsConsolidatedAsync_WithValidParameters_ReturnsNonNullList()
    {
        // Arrange - Global coordinates (Los Angeles)
        const string validLat = "34.0522";
        const string validLon = "-118.2437";
        const int radiusKm = 500;
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildEmscAlertsConsolidatedAsync(
            validLat, validLon, radiusKm, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task BuildEmscAlertsConsolidatedAsync_WithInvalidCoordinates_ReturnsEmptyList()
    {
        // Arrange
        const string invalidLat = "xyz";
        const string validLon = "-118.2437";
        const int radiusKm = 500;
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildEmscAlertsConsolidatedAsync(
            invalidLat, validLon, radiusKm, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BuildEmscAlertsConsolidatedAsync_WithDifferentRadius_AcceptsParameter()
    {
        // Arrange
        const string validLat = "34.0522";
        const string validLon = "-118.2437";
        const int radiusKm = 1000;
        var logger = NullLogger.Instance;

        // Act
        var result = await WeatherAlertHelper.BuildEmscAlertsConsolidatedAsync(
            validLat, validLon, radiusKm, logger, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
