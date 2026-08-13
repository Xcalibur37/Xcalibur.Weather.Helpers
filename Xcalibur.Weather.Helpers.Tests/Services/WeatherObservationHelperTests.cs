using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;

namespace Xcalibur.Weather.Helpers.Tests.Services;

public sealed class WeatherObservationHelperTests
{
    [Fact]
    public async Task GetObservationAsync_WithInvalidLatitude_ReturnsNull()
    {
        var result = await WeatherObservationHelper.GetObservationAsync(
            "invalid", "-74.0060", NullLogger.Instance, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMultipleObservationsAsync_WithInvalidLongitude_ReturnsEmptyList()
    {
        var result = await WeatherObservationHelper.GetMultipleObservationsAsync(
            "40.7128", "invalid", 5, NullLogger.Instance, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void DetermineRegion_WithInvalidCoordinates_ReturnsUnknown()
    {
        var result = WeatherObservationHelper.DetermineRegion("invalid", "invalid");

        result.Should().Be(Xcalibur.Weather.Models.Implementation.Observations.WeatherRegion.Unknown);
    }
}