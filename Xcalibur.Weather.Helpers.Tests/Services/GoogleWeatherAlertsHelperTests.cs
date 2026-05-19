using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models.Testing;
using Xcalibur.Weather.Services.WeatherProvider.GoogleWeatherAlerts;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests covering the Google Weather Alerts helper and service behavior.
    /// Note: GoogleWeatherAlertsHelper creates an internal HttpClient per call, so tests
    /// exercise the service directly with a stubbed HttpMessageHandler to verify
    /// deserialization and the behaviors the helper depends on.
    /// </summary>
    public sealed class GoogleWeatherAlertsHelperTests
    {
        private const string SampleJson =
            """
            {
              "regionCode": "US",
              "weatherAlerts": [
                {
                  "alertId": "urn:oid:2.49.0.1.840.0.abc123.001.1",
                  "alertTitle": { "text": "Red Flag (Fire Weather) Warning", "languageCode": "en" },
                  "eventType": "FIRE_WEATHER",
                  "areaName": "Rio Grande Valley",
                  "polygon": "{\"type\":\"Polygon\",\"coordinates\":[[[-106.27,35.61],[-106.30,35.65],[-106.27,35.61]]]}",
                  "description": "RED FLAG WARNING IN EFFECT FROM NOON TODAY TO 7 PM MDT THIS EVENING.",
                  "severity": "SEVERE",
                  "certainty": "LIKELY",
                  "urgency": "EXPECTED",
                  "instruction": [ "Please advise fire crews of this Red Flag Warning." ],
                  "timezoneOffset": "-21600s",
                  "startTime": "2026-05-19T07:03:00Z",
                  "expirationTime": "2026-05-20T01:00:00Z",
                  "dataSource": {
                    "publisher": "NOAA",
                    "name": "National Weather Service",
                    "authorityUri": "https://www.weather.gov/"
                  }
                }
              ]
            }
            """;

        [Fact]
        public async Task GoogleWeatherAlertsService_GetWeatherAlertsAsync_ShouldDeserializeResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SampleJson, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GoogleWeatherAlertsService(http, "DUMMY_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var result = await service.GetWeatherAlertsAsync("35.08", "-106.65", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.RegionCode.Should().Be("US");
            result.WeatherAlerts.Should().HaveCount(1);

            var alert = result.WeatherAlerts![0];
            alert.AlertId.Should().Be("urn:oid:2.49.0.1.840.0.abc123.001.1");
            alert.AlertTitle.Should().NotBeNull();
            alert.AlertTitle!.Text.Should().Be("Red Flag (Fire Weather) Warning");
            alert.AlertTitle.LanguageCode.Should().Be("en");
            alert.EventType.Should().Be("FIRE_WEATHER");
            alert.AreaName.Should().Be("Rio Grande Valley");
            alert.Severity.Should().Be("SEVERE");
            alert.Certainty.Should().Be("LIKELY");
            alert.Urgency.Should().Be("EXPECTED");
            alert.Instruction.Should().HaveCount(1);
            alert.StartTime.Should().Be("2026-05-19T07:03:00Z");
            alert.ExpirationTime.Should().Be("2026-05-20T01:00:00Z");
            alert.DataSource.Should().NotBeNull();
            alert.DataSource!.Publisher.Should().Be("NOAA");
            alert.DataSource.Name.Should().Be("National Weather Service");
            alert.DataSource.AuthorityUri.Should().Be("https://www.weather.gov/");
            alert.Polygon.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_GetWeatherAlertsAsync_ShouldReturnNull_WhenResponseIsNotSuccess()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var http = new HttpClient(new DelegatingHandlerStub(response));
            var service = new GoogleWeatherAlertsService(http, "DUMMY_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var result = await service.GetWeatherAlertsAsync("0", "0", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_GetWeatherAlertsAsync_ShouldReturnNull_WhenResponseIsInvalidJson()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response));
            var service = new GoogleWeatherAlertsService(http, "DUMMY_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var result = await service.GetWeatherAlertsAsync("0", "0", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_TestApiKey_ShouldReturnTrue_WhenResponseIsSuccess()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GoogleWeatherAlertsService(http, "VALID_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_TestApiKey_ShouldReturnFalse_WhenResponseIsUnauthorized()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GoogleWeatherAlertsService(http, "BAD_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_TestApiKey_ShouldReturnFalse_WhenResponseIsForbidden()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GoogleWeatherAlertsService(http, "BAD_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task BuildWeatherAlertsAsync_ShouldReturnNull_WhenApiKeyIsEmpty()
        {
            // Act
            var result = await GoogleWeatherAlertsHelper.BuildWeatherAlertsAsync(string.Empty, "35.08", "-106.65");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildWeatherAlertsAsync_ShouldReturnNull_WhenApiKeyIsWhiteSpace()
        {
            // Act
            var result = await GoogleWeatherAlertsHelper.BuildWeatherAlertsAsync("   ", "35.08", "-106.65");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_GetWeatherAlertsAsync_ShouldReturnEmptyAlerts_WhenWeatherAlertsIsEmpty()
        {
            // Arrange
            var json =
                """
                {
                  "regionCode": "US",
                  "weatherAlerts": []
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

            var service = new GoogleWeatherAlertsService(http, "DUMMY_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var result = await service.GetWeatherAlertsAsync("35.08", "-106.65", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.WeatherAlerts.Should().BeEmpty();
        }

        [Fact]
        public async Task GoogleWeatherAlertsService_GetWeatherAlertsAsync_ShouldDeserializeMultipleAlerts()
        {
            // Arrange
            var json =
                """
                {
                  "regionCode": "US",
                  "weatherAlerts": [
                    {
                      "alertId": "urn:oid:alert1",
                      "alertTitle": { "text": "Winter Storm Warning", "languageCode": "en" },
                      "eventType": "WINTER_STORM",
                      "severity": "EXTREME",
                      "certainty": "OBSERVED",
                      "urgency": "IMMEDIATE",
                      "instruction": [],
                      "startTime": "2026-01-10T12:00:00Z",
                      "expirationTime": "2026-01-11T12:00:00Z"
                    },
                    {
                      "alertId": "urn:oid:alert2",
                      "alertTitle": { "text": "Dense Fog Advisory", "languageCode": "en" },
                      "eventType": "FOG",
                      "severity": "MINOR",
                      "certainty": "LIKELY",
                      "urgency": "EXPECTED",
                      "instruction": [],
                      "startTime": "2026-01-10T06:00:00Z",
                      "expirationTime": "2026-01-10T10:00:00Z"
                    }
                  ]
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

            var service = new GoogleWeatherAlertsService(http, "DUMMY_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);

            // Act
            var result = await service.GetWeatherAlertsAsync("35.08", "-106.65", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.WeatherAlerts.Should().HaveCount(2);
            result.WeatherAlerts![0].EventType.Should().Be("WINTER_STORM");
            result.WeatherAlerts[1].EventType.Should().Be("FOG");
        }

        [Fact]
        public async Task WeatherAlertInformation_ShouldMapFieldsFromResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SampleJson, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GoogleWeatherAlertsService(http, "DUMMY_KEY", NullLogger<GoogleWeatherAlertsService>.Instance);
            var raw = await service.GetWeatherAlertsAsync("35.08", "-106.65", CancellationToken.None);

            // Act
            var info = new Xcalibur.Weather.Models.WeatherAlertInformation(raw);

            // Assert
            info.RegionCode.Should().Be("US");
            info.Alerts.Should().HaveCount(1);

            var item = info.Alerts[0];
            item.Title.Should().Be("Red Flag (Fire Weather) Warning");
            item.EventType.Should().Be("FIRE_WEATHER");
            item.AreaName.Should().Be("Rio Grande Valley");
            item.Severity.Should().Be("SEVERE");
            item.Certainty.Should().Be("LIKELY");
            item.Urgency.Should().Be("EXPECTED");
            item.Instructions.Should().HaveCount(1);
            item.Publisher.Should().Be("NOAA");
            item.StartTime.Should().NotBeNull();
            item.ExpirationTime.Should().NotBeNull();
            item.Polygon.Should().NotBeNullOrWhiteSpace();
        }
    }
}
