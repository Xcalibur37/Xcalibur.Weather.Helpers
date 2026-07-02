using Xcalibur.Weather.Models.Implementation.WeatherAlerts;
using Xcalibur.Weather.Models.Services.WeatherAlert;

namespace Xcalibur.Weather.Helpers.Tests.WeatherAlert;

/// <summary>
/// Tests for CombinedWeatherAlertInformation functionality.
/// </summary>
public class CombinedWeatherAlertInformationTests
{
    [Fact]
    public void Constructor_WithNoData_ShouldInitializeEmptyCollections()
    {
        // Act
        var combined = new CombinedWeatherAlertInformation();

        // Assert
        Assert.NotNull(combined.Alerts);
        Assert.Empty(combined.Alerts);
        Assert.NotNull(combined.DataSources);
        Assert.Empty(combined.DataSources);
        Assert.Null(combined.Latitude);
        Assert.Null(combined.Longitude);
        Assert.Null(combined.RegionCode);
        Assert.Null(combined.LastUpdated);
    }

    [Fact]
    public void Constructor_WithValidCoordinates_ShouldParseLatitudeAndLongitude()
    {
        // Arrange
        var latitude = "40.7128";
        var longitude = "-74.0060";

        // Act
        var combined = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, latitude, longitude);

        // Assert
        Assert.NotNull(combined.Latitude);
        Assert.NotNull(combined.Longitude);
        Assert.Equal(40.7128, combined.Latitude.Value, 4);
        Assert.Equal(-74.0060, combined.Longitude.Value, 4);
        Assert.NotNull(combined.LastUpdated);
    }

    [Theory]
    [InlineData("40.7128", "-74.0060")]
    [InlineData("51.5074", "-0.1278")]
    [InlineData("35.6762", "139.6503")]
    [InlineData("-33.8688", "151.2093")]
    public void Constructor_WithVariousCoordinates_ShouldParseCultureInvariant(string lat, string lon)
    {
        // Act
        var combined = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, lat, lon);

        // Assert
        Assert.NotNull(combined.Latitude);
        Assert.NotNull(combined.Longitude);
    }

    [Fact]
    public void Constructor_WithInvalidCoordinates_ShouldNotSetCoordinates()
    {
        // Arrange
        var latitude = "invalid";
        var longitude = "also-invalid";

        // Act
        var combined = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, latitude, longitude);

        // Assert
        Assert.Null(combined.Latitude);
        Assert.Null(combined.Longitude);
    }

    [Fact]
    public void Constructor_WithNullOrEmptyCoordinates_ShouldNotSetCoordinates()
    {
        // Act
        var combined1 = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, null, null);
        var combined2 = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, "", "");

        // Assert
        Assert.Null(combined1.Latitude);
        Assert.Null(combined1.Longitude);
        Assert.Null(combined2.Latitude);
        Assert.Null(combined2.Longitude);
    }

    [Fact]
    public void Constructor_WithNullAlertData_ShouldNotAddToDataSources()
    {
        // Act
        var combined = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, null, null);

        // Assert
        Assert.Empty(combined.Alerts);
        Assert.Empty(combined.DataSources);
    }

    [Fact]
    public void Constructor_ShouldSetLastUpdatedToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var combined = new CombinedWeatherAlertInformation(
            null, null, null, null, null, null, null, null, null);

        // Arrange
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(combined.LastUpdated);
        Assert.InRange(combined.LastUpdated.Value, before.AddSeconds(-1), after.AddSeconds(1));
    }
}
