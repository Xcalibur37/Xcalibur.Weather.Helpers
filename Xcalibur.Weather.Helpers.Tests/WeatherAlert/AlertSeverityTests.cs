using Xcalibur.Weather.Models.Implementation.WeatherAlerts;
using Xcalibur.Weather.Models.Services.WeatherAlert;

namespace Xcalibur.Weather.Helpers.Tests.WeatherAlert;

/// <summary>
/// Tests for AlertSeverity enum.
/// </summary>
public class AlertSeverityTests
{
    [Fact]
    public void AlertSeverity_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)AlertSeverity.Unknown);
        Assert.Equal(1, (int)AlertSeverity.Minor);
        Assert.Equal(2, (int)AlertSeverity.Moderate);
        Assert.Equal(3, (int)AlertSeverity.Severe);
        Assert.Equal(4, (int)AlertSeverity.Extreme);
    }

    [Fact]
    public void AlertSeverity_ShouldBeOrderedBySeverityLevel()
    {
        // Assert - Ensure ascending order represents increasing severity
        Assert.True(AlertSeverity.Unknown < AlertSeverity.Minor);
        Assert.True(AlertSeverity.Minor < AlertSeverity.Moderate);
        Assert.True(AlertSeverity.Moderate < AlertSeverity.Severe);
        Assert.True(AlertSeverity.Severe < AlertSeverity.Extreme);
    }

    [Theory]
    [InlineData(AlertSeverity.Unknown, "Unknown")]
    [InlineData(AlertSeverity.Minor, "Minor")]
    [InlineData(AlertSeverity.Moderate, "Moderate")]
    [InlineData(AlertSeverity.Severe, "Severe")]
    [InlineData(AlertSeverity.Extreme, "Extreme")]
    public void AlertSeverity_ToString_ShouldReturnCorrectName(AlertSeverity severity, string expectedName)
    {
        // Act
        var result = severity.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void AlertSeverity_ShouldHaveAllValuesDefinedInEnum()
    {
        // Act
        var allValues = Enum.GetValues<AlertSeverity>();

        // Assert
        Assert.Equal(5, allValues.Length);
        Assert.Contains(AlertSeverity.Unknown, allValues);
        Assert.Contains(AlertSeverity.Minor, allValues);
        Assert.Contains(AlertSeverity.Moderate, allValues);
        Assert.Contains(AlertSeverity.Severe, allValues);
        Assert.Contains(AlertSeverity.Extreme, allValues);
    }
}
