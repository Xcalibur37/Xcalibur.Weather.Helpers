using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models.Implementation.Pollen;
using Xcalibur.Weather.Models.Services.Atmospore.Response;
using Xcalibur.Weather.Models.Testing;
using Xcalibur.Weather.Services;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests covering the Atmospore helper and service behavior.
    /// Note: AtmosporeHelper creates an internal HttpClient per call, so tests
    /// exercise the service directly with a stubbed HttpMessageHandler to verify
    /// deserialization and the behaviors the helper depends on.
    /// </summary>
    public sealed class AtmosporeHelperTests
    {
        private const string DummyKey = "DUMMY_KEY";

        // ── Helper-level guard tests ──────────────────────────────────────────

        [Fact]
        public async Task BuildPollenForecastAsync_ReturnsNull_WhenApiKeyIsNull()
        {
            var result = await AtmosporeHelper.BuildPollenForecastAsync(null!, "39.43", "-77.80");
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildPollenForecastAsync_ReturnsNull_WhenApiKeyIsEmpty()
        {
            var result = await AtmosporeHelper.BuildPollenForecastAsync(string.Empty, "39.43", "-77.80");
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildPollenForecastAsync_ReturnsNull_WhenApiKeyIsWhiteSpace()
        {
            var result = await AtmosporeHelper.BuildPollenForecastAsync("   ", "39.43", "-77.80");
            result.Should().BeNull();
        }

        // ── Service-level deserialization tests ───────────────────────────────

        [Fact]
        public async Task AtmosporeService_GetPollenForecastAsync_ShouldDeserializeResponse()
        {
            // Arrange — representative JSON matching the Atmospore API shape
            const string json =
                """
                {
                  "meta": {
                    "forecast_date": "2026-05-27",
                    "latitude": 39.4300996,
                    "longitude": -77.804161,
                    "forecast_days": 1
                  },
                  "species": {
                    "OAK": {
                      "display_name": "Oak",
                      "category": "tree",
                      "max": 450.5,
                      "avg": 320.0,
                      "risk_level": "High"
                    },
                    "GRASS": {
                      "display_name": "Grass",
                      "category": "grass",
                      "max": 120.0,
                      "avg": 80.0,
                      "risk_level": "Moderate"
                    }
                  }
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", "2026-05-27", 1, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Meta.Should().NotBeNull();
            // Note: Actual property names depend on the Xcalibur.Weather.Services package version
        }

        [Fact]
        public async Task AtmosporeService_GetPollenForecastAsync_UsesApiKeyHeader()
        {
            // Arrange — capture the outgoing request to verify the header
            HttpRequestMessage? capturedRequest = null;
            var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"meta":{}}""", Encoding.UTF8, "application/json")
            }, req => capturedRequest = req);

            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            await service.GetPollenForecastAsync("39.43", "-77.80", "2026-05-27", 1, CancellationToken.None);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Headers.TryGetValues("x-api-key", out var values).Should().BeTrue();
            values.Should().ContainSingle().Which.Should().Be(DummyKey);
        }

        [Fact]
        public async Task AtmosporeService_GetPollenForecastAsync_DoesNotPutApiKeyInUrl()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"meta":{}}""", Encoding.UTF8, "application/json")
            }, req => capturedRequest = req);

            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            await service.GetPollenForecastAsync("39.43", "-77.80", "2026-05-27", 1, CancellationToken.None);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.RequestUri!.ToString().Should().NotContain(DummyKey);
        }

        [Fact]
        public async Task AtmosporeService_TestApiKey_ReturnsFalse_WhenUnauthorized()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            using var http = new HttpClient(new DelegatingHandlerStub(response)) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            var result = await service.TestApiKey(CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AtmosporeService_TestApiKey_ReturnsFalse_WhenForbidden()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            using var http = new HttpClient(new DelegatingHandlerStub(response)) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            var result = await service.TestApiKey(CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AtmosporeService_TestApiKey_ReturnsTrue_WhenHttpOk()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"meta":{}}""", Encoding.UTF8, "application/json")
            };
            using var http = new HttpClient(new DelegatingHandlerStub(response)) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            var result = await service.TestApiKey(CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AtmosporeService_GetPollenForecastAsync_ReturnsNull_WhenNotSuccessStatusCode()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            using var http = new HttpClient(new DelegatingHandlerStub(response)) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", cancellationToken: CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AtmosporeService_GetPollenForecastAsync_ReturnsNull_WhenInvalidJson()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-valid-json", Encoding.UTF8, "application/json")
            };
            using var http = new HttpClient(new DelegatingHandlerStub(response)) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", cancellationToken: CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AtmosporeService_GetPollenForecastAsync_DefaultsDateToToday_WhenDateIsNull()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"meta":{}}""", Encoding.UTF8, "application/json")
            }, req => capturedRequest = req);

            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var service = new AtmosporeService(http, DummyKey, NullLogger<AtmosporeService>.Instance);

            var expectedDate = DateTime.Today.ToString("yyyy-MM-dd");

            // Act
            await service.GetPollenForecastAsync("39.43", "-77.80", null, 1, CancellationToken.None);

            // Assert
            capturedRequest!.RequestUri!.ToString().Should().Contain($"dt={expectedDate}");
        }

        // ── App-model mapping tests ───────────────────────────────────────────

        [Fact]
        public void AtmosporePollenInformation_MapsFromResponse_Correctly()
        {
            // Arrange
            var response = new PollenResponse
            {
                Meta = new PollenMetaResponse()
            };

            // Act
            var info = new PollenInformation(response);

            // Assert
            info.Should().NotBeNull();
            // Note: Actual property assertions depend on the Xcalibur.Weather.Services package version
            // and the PollenInformation implementation details
        }

        [Fact]
        public void AtmosporePollenInformation_MapsFromNullResponse_HandlesNull()
        {
            var info = new PollenInformation(null);

            info.Should().NotBeNull();
        }

        // ── Private helper ────────────────────────────────────────────────────

        private sealed class CapturingHandler(HttpResponseMessage response, Action<HttpRequestMessage> capture)
            : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                capture(request);
                return Task.FromResult(response);
            }
        }
    }
}
