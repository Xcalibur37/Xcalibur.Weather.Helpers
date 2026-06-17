using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="WeatherAlertHelper"/> class.
    /// These tests focus on input validation and behavior patterns.
    /// Integration tests with actual API calls would require network stubbing frameworks
    /// which are outside the scope of these unit tests.
    /// </summary>
    public sealed class WeatherAlertHelperTests
    {
        #region BuildCombinedAlertsAsync - Input Validation Tests

        [Fact]
        public async Task BuildCombinedAlertsAsync_InvalidLatitude_ReturnsNull()
        {
            // Arrange
            const string invalidLat = "invalid";
            const string validLon = "-74.0060";
            var logger = NullLogger.Instance;

            // Act
            var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                invalidLat, validLon, logger, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_InvalidLongitude_ReturnsNull()
        {
            // Arrange
            const string validLat = "40.7128";
            const string invalidLon = "not-a-number";
            var logger = NullLogger.Instance;

            // Act
            var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                validLat, invalidLon, logger, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_EmptyLatitude_ReturnsNull()
        {
            // Arrange
            const string emptyLat = "";
            const string validLon = "-74.0060";
            var logger = NullLogger.Instance;

            // Act
            var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                emptyLat, validLon, logger, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_EmptyLongitude_ReturnsNull()
        {
            // Arrange
            const string validLat = "40.7128";
            const string emptyLon = "";
            var logger = NullLogger.Instance;

            // Act
            var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                validLat, emptyLon, logger, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_ValidCoordinates_DoesNotThrow()
        {
            // Arrange - New York City coordinates
            const string lat = "40.7128";
            const string lon = "-74.0060";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert - Should not throw, though result may be null if no alerts exist
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_WithProvinceCode_DoesNotThrow()
        {
            // Arrange - Toronto coordinates with explicit province
            const string lat = "43.6532";
            const string lon = "-79.3832";
            const string provinceCode = "on";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None, provinceCode: provinceCode);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_WithStateCode_DoesNotThrow()
        {
            // Arrange - Sydney coordinates with explicit state
            const string lat = "-33.8688";
            const string lon = "151.2093";
            const string stateCode = "nsw";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None, stateCode: stateCode);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildMeteoalarmAlertsAsync Tests

        [Fact]
        public async Task BuildMeteoalarmAlertsAsync_ValidCoordinates_DoesNotThrow()
        {
            // Arrange - Paris coordinates (Europe)
            const string lat = "48.8566";
            const string lon = "2.3522";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildMeteoalarmAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildNwsAlertsAsync Tests

        [Fact]
        public async Task BuildNwsAlertsAsync_ValidCoordinates_DoesNotThrow()
        {
            // Arrange - Washington DC coordinates (US)
            const string lat = "38.9072";
            const string lon = "-77.0369";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildNwsAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildGdacsAlertsAsync Tests

        [Fact]
        public async Task BuildGdacsAlertsAsync_ValidCoordinates_DoesNotThrow()
        {
            // Arrange - Tokyo coordinates (global service)
            const string lat = "35.6762";
            const string lon = "139.6503";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildGdacsAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildEnvironmentCanadaAlertsAsync Tests

        [Fact]
        public async Task BuildEnvironmentCanadaAlertsAsync_ValidCoordinatesAndProvince_DoesNotThrow()
        {
            // Arrange - Toronto coordinates with Ontario province code
            const string lat = "43.6532";
            const string lon = "-79.3832";
            const string provinceCode = "on";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildEnvironmentCanadaAlertsAsync(
                lat, lon, provinceCode, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildEnvironmentCanadaAlertsAsync_ValidCoordinatesWithBC_DoesNotThrow()
        {
            // Arrange - Vancouver coordinates with BC province code
            const string lat = "49.2827";
            const string lon = "-123.1207";
            const string provinceCode = "bc";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildEnvironmentCanadaAlertsAsync(
                lat, lon, provinceCode, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildBomAlertsAsync Tests

        [Fact]
        public async Task BuildBomAlertsAsync_ValidCoordinatesAndState_DoesNotThrow()
        {
            // Arrange - Sydney coordinates with NSW state code
            const string lat = "-33.8688";
            const string lon = "151.2093";
            const string stateCode = "nsw";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildBomAlertsAsync(
                lat, lon, stateCode, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildBomAlertsAsync_ValidCoordinatesWithVIC_DoesNotThrow()
        {
            // Arrange - Melbourne coordinates with VIC state code
            const string lat = "-37.8136";
            const string lon = "144.9631";
            const string stateCode = "vic";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildBomAlertsAsync(
                lat, lon, stateCode, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildEmscAlertsAsync Tests

        [Fact]
        public async Task BuildEmscAlertsAsync_ValidCoordinatesAndRadius_DoesNotThrow()
        {
            // Arrange - Rome coordinates (earthquake monitoring)
            const string lat = "41.9028";
            const string lon = "12.4964";
            const int radiusKm = 500;
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildEmscAlertsAsync(
                lat, lon, radiusKm, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildEmscAlertsAsync_SmallRadius_DoesNotThrow()
        {
            // Arrange - Athens coordinates with small radius
            const string lat = "37.9838";
            const string lon = "23.7275";
            const int radiusKm = 100;
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildEmscAlertsAsync(
                lat, lon, radiusKm, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildEmscAlertsAsync_LargeRadius_DoesNotThrow()
        {
            // Arrange - Istanbul coordinates with large radius
            const string lat = "41.0082";
            const string lon = "28.9784";
            const int radiusKm = 1000;
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildEmscAlertsAsync(
                lat, lon, radiusKm, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region BuildDwdAlertsAsync Tests

        [Fact]
        public async Task BuildDwdAlertsAsync_ValidGermanCoordinates_DoesNotThrow()
        {
            // Arrange - Berlin coordinates (Germany)
            const string lat = "52.5200";
            const string lon = "13.4050";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildDwdAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildDwdAlertsAsync_MunichCoordinates_DoesNotThrow()
        {
            // Arrange - Munich coordinates (Germany)
            const string lat = "48.1351";
            const string lon = "11.5820";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildDwdAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Cancellation Token Tests

        [Fact]
        public async Task BuildCombinedAlertsAsync_WithCancelledToken_ThrowsOrReturnsQuickly()
        {
            // Arrange
            const string lat = "40.7128";
            const string lon = "-74.0060";
            var logger = NullLogger.Instance;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            try
            {
                var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                    lat, lon, logger, cts.Token);

                // If no exception is thrown, that's acceptable - the method may return null
                // when cancelled or handle cancellation gracefully
                result.Should().BeNull("because the operation was cancelled");
            }
            catch (OperationCanceledException)
            {
                // This is also acceptable - the method properly threw when cancelled
                Assert.True(true, "Method correctly threw OperationCanceledException");
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task BuildCombinedAlertsAsync_BoundaryCoordinates_DoesNotThrow()
        {
            // Arrange - North Pole (extreme latitude)
            const string lat = "90.0";
            const string lon = "0.0";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_NegativeCoordinates_DoesNotThrow()
        {
            // Arrange - Southern hemisphere, western longitude
            const string lat = "-45.0";
            const string lon = "-170.0";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_EquatorPrimeMeridian_DoesNotThrow()
        {
            // Arrange - Intersection of equator and prime meridian
            const string lat = "0.0";
            const string lon = "0.0";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_WithDecimalPrecision_DoesNotThrow()
        {
            // Arrange - High precision coordinates
            const string lat = "40.71278944";
            const string lon = "-74.00597357";
            var logger = NullLogger.Instance;

            // Act
            var act = async () => await WeatherAlertHelper.BuildCombinedAlertsAsync(
                lat, lon, logger, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion
    }
}
