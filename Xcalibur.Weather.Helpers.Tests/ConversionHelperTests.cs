using FluentAssertions;
using Xcalibur.Weather.Models;

namespace Xcalibur.Weather.Helpers.Tests
{
    /// <summary>
    /// Tests for the <see cref="ConversionHelper"/> class.
    /// </summary>
    public sealed class ConversionHelperTests
    {
        [Fact]
        public void FahrenheitToCelsius_Double_ConvertsAndRounds()
        {
            // Arrange
            double f = 98.6;

            // Act
            var c = f.FahrenheitToCelsius();

            // Assert
            c.Should().BeApproximately(37.0, 0.0001);
        }

        [Fact]
        public void FahrenheitToCelsius_Nullable_ReturnsDefaultWhenNull()
        {
            // Arrange
            double? f = null;

            // Act
            var c = f.FahrenheitToCelsius(defaultValue: 123.4);

            // Assert
            c.Should().Be(123.4);
        }

        [Fact]
        public void CelsiusToFahrenheit_Double_ConvertsAndRounds()
        {
            // Arrange
            double c = 37.0;

            // Act
            var f = c.CelsiusToFahrenheit();

            // Assert
            f.Should().BeApproximately(98.6, 0.0001);
        }

        [Fact]
        public void ConvertWindSpeed_ConvertsToAllUnits()
        {
            // Arrange
            double kmh = 100.0;

            // Act
            var mph = kmh.ConvertWindSpeed(WindSpeedUnits.Mph);
            var ftsec = kmh.ConvertWindSpeed(WindSpeedUnits.FtSec);
            var msec = kmh.ConvertWindSpeed(WindSpeedUnits.MSec);
            var knots = kmh.ConvertWindSpeed(WindSpeedUnits.Knots);
            var kmhr = kmh.ConvertWindSpeed(WindSpeedUnits.KmHr);

            // Assert (values based on constants in helper)
            mph.Should().BeApproximately(62.1371, 1e-4);
            ftsec.Should().BeApproximately(91.1344, 1e-4);
            msec.Should().BeApproximately(27.7778, 1e-4);
            knots.Should().BeApproximately(53.9957, 1e-4);
            kmhr.Should().BeApproximately(100.0, 1e-8);
        }

        [Fact]
        public void ConvertWindSpeed_NullUnitOrNullSpeed_ReturnsZero()
        {
            // Arrange
            double? speedNull = null;

            // Act
            var fromNullSpeed = speedNull.ConvertWindSpeed(WindSpeedUnits.Mph);
            var fromNullUnit = 10.0.ConvertWindSpeed((WindSpeedUnits?)null);

            // Assert
            fromNullSpeed.Should().Be(0);
            fromNullUnit.Should().Be(0);
        }

        [Fact]
        public void FormatTemperature_CelsiusAndFahrenheit_WithAndWithoutUnit()
        {
            // Arrange
            double tempC = 0.0;

            // Act
            var cDisplay = tempC.FormatTemperature(TemperatureUnits.Celsius, includeUnit: true);
            var fDisplay = tempC.FormatTemperature(TemperatureUnits.Fahrenheit, includeUnit: true);
            var noUnit = tempC.FormatTemperature(TemperatureUnits.Celsius, includeUnit: false);

            // Assert
            cDisplay.Should().Be("0°C");
            fDisplay.Should().Be("32°F");
            noUnit.Should().Be("0°");
        }

        [Fact]
        public void FormatTemperature_NullableReturnsEmptyWhenNull()
        {
            // Arrange
            double? temp = null;

            // Act
            var s = temp.FormatTemperature(TemperatureUnits.Celsius, includeUnit: true);

            // Assert
            s.Should().BeEmpty();
        }

        [Fact]
        public void FormatLength_MetricAndImperial_FormatsCorrectly()
        {
            // Arrange
            double? mm = 1000.0; // 1 meter -> 39.3701 inches

            // Act
            var metricWithUnit = mm.FormatLength(DistanceUnits.Metric, includeUnit: true);
            var imperialWithUnit = mm.FormatLength(DistanceUnits.Imperial, includeUnit: true);
            var imperialNoUnit = mm.FormatLength(DistanceUnits.Imperial, includeUnit: false);

            // Assert
            metricWithUnit.Should().Be("1,000.00 mm");
            imperialWithUnit.Should().Be("39.37 in");
            imperialNoUnit.Should().Be("39.37");
        }

        [Fact]
        public void FormatLength_NullValue_ReturnsEmpty()
        {
            // Act
            var s = ((double?)null).FormatLength(DistanceUnits.Metric, includeUnit: true);

            // Assert
            s.Should().BeEmpty();
        }

        [Fact]
        public void FormatPressure_HPa_InHg_MmHg_FormatsCorrectly()
        {
            // Arrange
            double? hpa = 1013.25;

            // Act
            var hpaStr = hpa.FormatPressure(BarometerUnits.HPa, includeUnit: true);
            var inHgStr = hpa.FormatPressure(BarometerUnits.InHg, includeUnit: true);
            var mmHgStr = hpa.FormatPressure(BarometerUnits.MmHg, includeUnit: true);

            // Assert
            hpaStr.Should().Be("1013.25 hPa");
            inHgStr.Should().Be("29.92 inHg");
            mmHgStr.Should().Be("760.00 mmHg");
        }

        [Fact]
        public void FormatPressure_NullValue_ReturnsEmpty()
        {
            // Act
            var s = ((double?)null).FormatPressure(BarometerUnits.HPa, includeUnit: true);

            // Assert
            s.Should().BeEmpty();
        }
    }
}