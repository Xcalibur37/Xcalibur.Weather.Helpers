using Xcalibur.Weather.Models.Implementation.WeatherAlerts;

namespace Xcalibur.Weather.Helpers.Tests.WeatherAlert;

/// <summary>
/// Tests for AlertMapper functionality.
/// </summary>
public class AlertMapperTests
{
    [Theory]
    [InlineData("Immediate", AlertUrgency.Immediate)]
    [InlineData("Expected", AlertUrgency.Expected)]
    [InlineData("Future", AlertUrgency.Future)]
    [InlineData("Past", AlertUrgency.Past)]
    [InlineData("", AlertUrgency.Unknown)]
    [InlineData(null, AlertUrgency.Unknown)]
    public void MapUrgency_ShouldMapCorrectly(string? input, AlertUrgency expected)
    {
        // Act
        var result = AlertMapper.MapUrgency(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Observed", AlertCertainty.Observed)]
    [InlineData("Likely", AlertCertainty.Likely)]
    [InlineData("Possible", AlertCertainty.Possible)]
    [InlineData("Unlikely", AlertCertainty.Unlikely)]
    [InlineData("", AlertCertainty.Unknown)]
    [InlineData(null, AlertCertainty.Unknown)]
    public void MapCertainty_ShouldMapCorrectly(string? input, AlertCertainty expected)
    {
        // Act
        var result = AlertMapper.MapCertainty(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Extreme", AlertSeverity.Extreme)]
    [InlineData("Severe", AlertSeverity.Severe)]
    [InlineData("Moderate", AlertSeverity.Moderate)]
    [InlineData("Minor", AlertSeverity.Minor)]
    [InlineData("", AlertSeverity.Unknown)]
    [InlineData(null, AlertSeverity.Unknown)]
    [InlineData("extreme", AlertSeverity.Extreme)]
    [InlineData("SEVERE", AlertSeverity.Severe)]
    public void MapSeverity_ShouldMapCorrectly(string? input, AlertSeverity expected)
    {
        // Act
        var result = AlertMapper.MapSeverity(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Met", AlertCategory.Met)]
    [InlineData("Geo", AlertCategory.Geo)]
    [InlineData("Fire", AlertCategory.Fire)]
    [InlineData("Health", AlertCategory.Health)]
    [InlineData("", AlertCategory.Unknown)]
    [InlineData(null, AlertCategory.Unknown)]
    public void MapCategory_ShouldMapCorrectly(string? input, AlertCategory expected)
    {
        // Act
        var result = AlertMapper.MapCategory(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Tornado Warning", AlertEventType.Tornado)]
    [InlineData("tornado", AlertEventType.Tornado)]
    [InlineData("Hurricane Watch", AlertEventType.Hurricane)]
    [InlineData("Severe Thunderstorm Warning", AlertEventType.SevereThunderstorm)]
    [InlineData("Flash Flood Warning", AlertEventType.FlashFlood)]
    [InlineData("Winter Storm Watch", AlertEventType.WinterStorm)]
    [InlineData("Blizzard Warning", AlertEventType.Blizzard)]
    [InlineData("Heat Advisory", AlertEventType.Heat)]
    [InlineData("earthquake", AlertEventType.Earthquake)]
    [InlineData("", AlertEventType.Unknown)]
    [InlineData(null, AlertEventType.Unknown)]
    public void MapEventType_ShouldMapCorrectly(string? input, AlertEventType expected)
    {
        // Act
        var result = AlertMapper.MapEventType(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MapEventType_ShouldHandleCaseInsensitivity()
    {
        // Arrange
        var inputs = new[] { "TORNADO", "tornado", "Tornado", "ToRnAdO" };

        // Act & Assert
        foreach (var input in inputs)
        {
            var result = AlertMapper.MapEventType(input);
            Assert.Equal(AlertEventType.Tornado, result);
        }
    }

    [Fact]
    public void MapEventType_ShouldIgnoreCommonSuffixes()
    {
        // Arrange & Act
        var warning = AlertMapper.MapEventType("Tornado Warning");
        var watch = AlertMapper.MapEventType("Tornado Watch");
        var advisory = AlertMapper.MapEventType("Tornado Advisory");

        // Assert
        Assert.Equal(AlertEventType.Tornado, warning);
        Assert.Equal(AlertEventType.Tornado, watch);
        Assert.Equal(AlertEventType.Tornado, advisory);
    }
}
