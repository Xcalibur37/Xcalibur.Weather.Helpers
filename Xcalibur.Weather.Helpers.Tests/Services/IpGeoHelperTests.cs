using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.Testing;
using Xcalibur.Weather.Models.WeatherProvider.IpGeo.Astronomy;
using Xcalibur.Weather.Services.WeatherProvider.IpGeo;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests covering the IP Geolocation / Astronomy helpers and service behavior.
    /// Note: IpGeoHelper creates an internal HttpClient in its implementation which
    /// makes direct isolation via reflection/field swap impractical. The tests below
    /// therefore:
    ///  - verify the mapping logic (AstronomyModel -> SunMoonPoint) via the model ctor
    ///  - exercise the IpGeoService using a stubbed HttpMessageHandler to verify deserialization
    ///    and behavior the helper depends on.
    /// </summary>
    public sealed class IpGeoHelperTests
    {
        [Fact]
        public void SunMoonPoint_ShouldMapValues_FromAstronomyModel()
        {
            // Arrange - create an AstronomyModel with representative values
            var astronomy = new AstronomyModel
            {
                Sunrise = "06:00",
                Sunset = "18:00",
                DayLength = "12:00:00",
                Moonrise = "20:00",
                Moonset = "06:00",
                MoonPhase = "Waxing Gibbous",
                MoonIlluminationPercentage = "65",
                MoonAngle = 42.5
            };

            // Act
            var point = new SunMoonPoint(astronomy);

            // Assert - values are mapped one-to-one
            point.Sunrise.Should().Be("06:00");
            point.Sunset.Should().Be("18:00");
            point.DayLength.Should().Be("12:00:00");
            point.Moonrise.Should().Be("20:00");
            point.Moonset.Should().Be("06:00");
            point.MoonPhase.Should().Be("Waxing Gibbous");
            point.MoonIlluminationPercentage.Should().Be("65");
        }

        [Fact]
        public async Task IpGeoService_GetSunMoonDataAsync_ShouldDeserializeResponse()
        {
            // Arrange - valid JSON that matches SunMoonDataResponse shape
            var json =
                """
                {
                  "location": { "latitude": "12.34", "longitude": "56.78" },
                  "astronomy": {
                    "sunrise": "06:01",
                    "sunset": "18:02",
                    "day_length": "12:01:00",
                    "moonrise": "20:10",
                    "moonset": "06:05",
                    "moon_phase": "Full Moon",
                    "moon_illumination_percentage": "99",
                    "moon_angle": 12.34
                  }
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Use a Null logger since the service requires one
            var logger = NullLogger<IpGeoService>.Instance;
            var service = new IpGeoService(http, "DUMMY_KEY", logger);

            // Act
            var result = await service.GetSunMoonDataAsync("12.34", "56.78", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Astronomy.Should().NotBeNull();

            var astro = result.Astronomy!;
            astro.Sunrise.Should().Be("06:01");
            astro.Sunset.Should().Be("18:02");
            astro.DayLength.Should().Be("12:01:00");
            astro.Moonrise.Should().Be("20:10");
            astro.Moonset.Should().Be("06:05");
            astro.MoonPhase.Should().Be("Full Moon");
            astro.MoonIlluminationPercentage.Should().Be("99");
            astro.MoonAngle.Should().Be(12.34);
        }

        [Fact]
        public async Task IpGeoService_TestApiKey_ShouldReturnTrue_WhenResponseIsSuccess()
        {
            // Arrange — any 2xx response means the key is valid
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new IpGeoService(http, "VALID_KEY", NullLogger<IpGeoService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public async Task IpGeoService_TestApiKey_ShouldReturnFalse_WhenResponseIsUnauthorized()
        {
            // Arrange — 401 means invalid key
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new IpGeoService(http, "BAD_KEY", NullLogger<IpGeoService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task BuildSunMoonPoint_ShouldReturnNull_WhenApiKeyIsEmpty()
        {
            // Act — empty key should short-circuit before any HTTP call
            var point = await IpGeoHelper.BuildSunMoonPointAsync(
                ipGeoApiKey: "",
                latitude: "12.34",
                longitude: "56.78",
                logger: NullLogger.Instance);

            // Assert
            point.Should().BeNull();
        }
    }
}