using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using ObservationWeatherRegion = Xcalibur.Weather.Models.Implementation.Observations.WeatherRegion;

namespace Xcalibur.Weather.Helpers.Tests.Services;

public sealed class WeatherObservationHelperTests
{
    #region GetObservationAsync (string overload)

    [Fact]
    public async Task GetObservationAsync_WithInvalidLatitude_ReturnsNull()
    {
        var result = await WeatherObservationHelper.GetObservationAsync(
            "invalid", "-74.0060", NullLogger.Instance, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetObservationAsync_WithInvalidLongitude_ReturnsNull()
    {
        var result = await WeatherObservationHelper.GetObservationAsync(
            "40.7128", "invalid", NullLogger.Instance, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetObservationAsync_WithBothInvalidCoordinates_ReturnsNull()
    {
        var result = await WeatherObservationHelper.GetObservationAsync(
            "abc", "xyz", NullLogger.Instance, CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetMultipleObservationsAsync (string overload)

    [Fact]
    public async Task GetMultipleObservationsAsync_WithInvalidLongitude_ReturnsEmptyList()
    {
        var result = await WeatherObservationHelper.GetMultipleObservationsAsync(
            "40.7128", "invalid", 5, NullLogger.Instance, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMultipleObservationsAsync_WithInvalidLatitude_ReturnsEmptyList()
    {
        var result = await WeatherObservationHelper.GetMultipleObservationsAsync(
            "invalid", "-74.0060", 5, NullLogger.Instance, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMultipleObservationsAsync_WithBothInvalidCoordinates_ReturnsEmptyList()
    {
        var result = await WeatherObservationHelper.GetMultipleObservationsAsync(
            "abc", "xyz", 5, NullLogger.Instance, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region DetermineRegion (string overload)

    [Fact]
    public void DetermineRegion_WithInvalidCoordinates_ReturnsUnknown()
    {
        var result = WeatherObservationHelper.DetermineRegion("invalid", "invalid");

        result.Should().Be(ObservationWeatherRegion.Unknown);
    }

    [Fact]
    public void DetermineRegion_WithInvalidLatitudeOnly_ReturnsUnknown()
    {
        var result = WeatherObservationHelper.DetermineRegion("invalid", "-74.0060");

        result.Should().Be(ObservationWeatherRegion.Unknown);
    }

    [Fact]
    public void DetermineRegion_WithInvalidLongitudeOnly_ReturnsUnknown()
    {
        var result = WeatherObservationHelper.DetermineRegion("40.7128", "invalid");

        result.Should().Be(ObservationWeatherRegion.Unknown);
    }

    [Fact]
    public void DetermineRegion_WithValidCoordinates_ReturnsKnownRegion()
    {
        // New York City — should resolve to a non-Unknown region
        var result = WeatherObservationHelper.DetermineRegion("40.7128", "-74.0060");

        result.Should().NotBe(ObservationWeatherRegion.Unknown);
    }

    #endregion

    #region DetermineRegion (double overload)

    [Fact]
    public void DetermineRegion_Double_WithNycCoordinates_ReturnsNonUnknown()
    {
        var result = WeatherObservationHelper.DetermineRegion(40.7128, -74.0060);

        result.Should().NotBe(ObservationWeatherRegion.Unknown);
    }

    [Fact]
    public void DetermineRegion_Double_WithTokyoCoordinates_ReturnsNonUnknown()
    {
        // Tokyo, Japan
        var result = WeatherObservationHelper.DetermineRegion(35.6762, 139.6503);

        result.Should().NotBe(ObservationWeatherRegion.Unknown);
    }

    [Fact]
    public void DetermineRegion_Double_WithSydneyCoordinates_ReturnsNonUnknown()
    {
        // Sydney, Australia
        var result = WeatherObservationHelper.DetermineRegion(-33.8688, 151.2093);

        result.Should().NotBe(ObservationWeatherRegion.Unknown);
    }

    #endregion
}
