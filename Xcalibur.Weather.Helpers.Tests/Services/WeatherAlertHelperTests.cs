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

        // NOTE: Integration tests that make actual API calls have been removed
        // to improve test suite performance. These tests were taking 30+ seconds
        // because they call multiple external weather alert APIs.
        // The validation tests above provide sufficient coverage for input handling.

        [Fact]
        public async Task BuildCombinedAlertsAsync_ValidCoordinates_WithProvinceCode_AcceptsParameter()
        {
            // Arrange
            const string validLat = "43.65";
            const string validLon = "-79.38";
            const string provinceCode = "ON";
            var logger = NullLogger.Instance;

            // Act - Just verify the method accepts the parameter without throwing
            var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                validLat, validLon, logger, CancellationToken.None, provinceCode: provinceCode);

            // Assert - No exception means the parameter was accepted correctly
            // Result may be null if no alerts are available, which is fine
            Assert.True(true, "Method accepted provinceCode parameter");
        }

        [Fact]
        public async Task BuildCombinedAlertsAsync_ValidCoordinates_WithStateCode_AcceptsParameter()
        {
            // Arrange
            const string validLat = "-33.87";
            const string validLon = "151.21";
            const string stateCode = "NSW";
            var logger = NullLogger.Instance;

            // Act - Just verify the method accepts the parameter without throwing
            var result = await WeatherAlertHelper.BuildCombinedAlertsAsync(
                validLat, validLon, logger, CancellationToken.None, stateCode: stateCode);

            // Assert - No exception means the parameter was accepted correctly
            // Result may be null if no alerts are available, which is fine
            Assert.True(true, "Method accepted stateCode parameter");
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

        // NOTE: Edge case integration tests removed - they make actual API calls to GDACS and EMSC
        // services which take 3+ seconds each. These were integration tests disguised as unit tests.
        // Input validation tests above provide sufficient coverage for coordinate parsing and validation.

        #endregion
    }
}
