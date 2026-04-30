using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.Testing;
using Xcalibur.Weather.Models.WeatherProvider.SunriseSunset;
using Xcalibur.Weather.Services.WeatherProvider.SunriseSunset;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests covering the SunriseSunset.io helper and service behavior.
    /// </summary>
    public sealed class SunriseSunsetHelperTests
    {
        [Fact]
        public void SunMoonPoint_ShouldMapValues_FromSunriseSunsetResultModel()
        {
            // Arrange
            var result = new SunriseSunsetResultModel
            {
                Sunrise = "6:14:04 AM",
                Sunset = "8:05:25 PM",
                DayLength = "13:51:20",
                Moonrise = "7:31:39 PM",
                Moonset = "5:27:04 AM",
                MoonPhase = "Full Moon",
                MoonIllumination = 97.61,
                MoonPhaseValue = 0.45
            };

            // Act
            var point = new SunMoonPoint(result);

            // Assert
            point.Sunrise.Should().Be("6:14:04 AM");
            point.Sunset.Should().Be("8:05:25 PM");
            point.DayLength.Should().Be("13:51:20");
            point.Moonrise.Should().Be("7:31:39 PM");
            point.Moonset.Should().Be("5:27:04 AM");
            point.MoonPhase.Should().Be("Full Moon");
            point.MoonIlluminationPercentage.Should().Be("97.61");
        }

        [Fact]
        public async Task SunriseSunsetService_GetSunriseSunsetAsync_ShouldDeserializeResponse()
        {
            // Arrange — representative JSON matching the real API shape
            var json =
                """
                {
                  "results": {
                    "date": "2026-04-30",
                    "sunrise": "6:14:04 AM",
                    "sunset": "8:05:25 PM",
                    "first_light": "4:32:24 AM",
                    "last_light": "9:47:05 PM",
                    "dawn": "5:45:08 AM",
                    "dusk": "8:34:21 PM",
                    "solar_noon": "1:09:45 PM",
                    "golden_hour": "7:28:24 PM",
                    "day_length": "13:51:20",
                    "nautical_twilight_begin": "5:10:02 AM",
                    "nautical_twilight_end": "9:09:28 PM",
                    "timezone": "America/New_York",
                    "utc_offset": -240,
                    "sun_altitude": 65.05,
                    "sun_azimuth": 180.48,
                    "sunrise_azimuth": 70.25,
                    "sunset_azimuth": 290.16,
                    "moonrise": "7:31:39 PM",
                    "moonset": "5:27:04 AM",
                    "moon_illumination": 97.61,
                    "moon_phase": "Full Moon",
                    "moon_phase_value": 0.45,
                    "moon_always_up": false,
                    "moon_always_down": false,
                    "elevation": 116
                  },
                  "status": "OK",
                  "tzid": "America/New_York"
                }
                """;

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(httpResponse))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var logger = NullLogger<SunriseSunsetService>.Instance;
            var service = new SunriseSunsetService(http, logger);

            // Act
            var result = await service.GetSunriseSunsetAsync("39.4300996", "-77.804161", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be("OK");
            result.Results.Should().NotBeNull();

            var r = result.Results!;
            r.Sunrise.Should().Be("6:14:04 AM");
            r.Sunset.Should().Be("8:05:25 PM");
            r.DayLength.Should().Be("13:51:20");
            r.Moonrise.Should().Be("7:31:39 PM");
            r.Moonset.Should().Be("5:27:04 AM");
            r.MoonPhase.Should().Be("Full Moon");
            r.MoonIllumination.Should().BeApproximately(97.61, 1e-6);
            r.MoonPhaseValue.Should().BeApproximately(0.45, 1e-6);
        }

        [Fact]
        public async Task SunriseSunsetService_GetSunriseSunsetAsync_ReturnsNull_WhenStatusNotSuccess()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var http = new HttpClient(new DelegatingHandlerStub(httpResponse));
            var service = new SunriseSunsetService(http, NullLogger<SunriseSunsetService>.Instance);

            // Act
            var result = await service.GetSunriseSunsetAsync("0", "0", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SunriseSunsetService_GetSunriseSunsetAsync_ReturnsNull_WhenResponseInvalidJson()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json")
            };
            var http = new HttpClient(new DelegatingHandlerStub(httpResponse));
            var service = new SunriseSunsetService(http, NullLogger<SunriseSunsetService>.Instance);

            // Act
            var result = await service.GetSunriseSunsetAsync("0", "0", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildSunMoonPoint_ShouldReturnMappedPoint_WhenServiceRespondsWithValidData()
        {
            // Arrange — inject a valid response via the public helper's internal HttpClient.
            // SunriseSunsetHelper creates a new HttpClient each call so we test the mapping through
            // the service directly and verify SunMoonPoint is correctly built.
            var json =
                """
                {
                  "results": {
                    "date": "2026-05-01",
                    "sunrise": "6:00:00 AM",
                    "sunset": "8:00:00 PM",
                    "first_light": "4:20:00 AM",
                    "last_light": "9:40:00 PM",
                    "dawn": "5:35:00 AM",
                    "dusk": "8:25:00 PM",
                    "solar_noon": "1:00:00 PM",
                    "golden_hour": "7:15:00 PM",
                    "day_length": "14:00:00",
                    "nautical_twilight_begin": "5:00:00 AM",
                    "nautical_twilight_end": "9:00:00 PM",
                    "timezone": "America/New_York",
                    "utc_offset": -240,
                    "sun_altitude": 50.0,
                    "sun_azimuth": 180.0,
                    "sunrise_azimuth": 68.0,
                    "sunset_azimuth": 292.0,
                    "moonrise": "8:00:00 PM",
                    "moonset": "5:00:00 AM",
                    "moon_illumination": 50.0,
                    "moon_phase": "First Quarter",
                    "moon_phase_value": 0.25,
                    "moon_always_up": false,
                    "moon_always_down": false,
                    "elevation": 100
                  },
                  "status": "OK",
                  "tzid": "America/New_York"
                }
                """;

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(httpResponse))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new SunriseSunsetService(http, NullLogger<SunriseSunsetService>.Instance);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await service.GetSunriseSunsetAsync("39.43", "-77.80", cts.Token);

            var point = response?.Results is null ? null : new SunMoonPoint(response.Results);

            // Assert
            point.Should().NotBeNull();
            point!.Sunrise.Should().Be("6:00:00 AM");
            point.Sunset.Should().Be("8:00:00 PM");
            point.DayLength.Should().Be("14:00:00");
            point.Moonrise.Should().Be("8:00:00 PM");
            point.Moonset.Should().Be("5:00:00 AM");
            point.MoonPhase.Should().Be("First Quarter");
            point.MoonIlluminationPercentage.Should().Be("50.00");
        }
    }
}
