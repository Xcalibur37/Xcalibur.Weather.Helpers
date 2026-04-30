using System.Net;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models.Helpers;
using Xcalibur.Weather.Models.Testing;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="OpenStreetMapHelper"/>.
    /// </summary>
    public sealed class OpenStreetMapHelperTests
    {
        private readonly FieldInfo _sharedClientField;
        private readonly HttpClient? _originalClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenStreetMapHelperTests"/> class.
        /// </summary>
        /// <exception cref="Exception">SharedHttpClient field not found</exception>
        public OpenStreetMapHelperTests()
        {
            _sharedClientField = typeof(OpenStreetMapHelper).GetField("_sharedHttpClient", BindingFlags.Static | BindingFlags.NonPublic)
                                 ?? throw new Exception("_sharedHttpClient field not found");
            _originalClient = (HttpClient?)_sharedClientField.GetValue(null);
        }

        /// <summary>
        /// Replaces the shared HTTP client with a stub.
        /// </summary>
        private void ReplaceSharedHttpClient(HttpMessageHandler handler)
        {
            var replacement = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            _sharedClientField.SetValue(null, replacement);
        }

        /// <summary>
        /// Restores the original shared HTTP client.
        /// </summary>
        private void RestoreOriginalHttpClient()
        {
            _sharedClientField.SetValue(null, _originalClient);
        }

        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldReturnMappedLocations_WhenNominatimReturnsResults()
        {
            // Arrange — representative Nominatim JSON array with one result
            var json =
                """
                [
                  {
                    "place_id": 1234,
                    "lat": "38.8977",
                    "lon": "-77.0366",
                    "display_name": "The White House, Pennsylvania Avenue NW, Washington, DC, 20500, United States",
                    "addresstype": "historic",
                    "importance": 0.9,
                    "address": {
                      "city": "Washington",
                      "county": "District of Columbia",
                      "state": "District of Columbia",
                      "postcode": "20500",
                      "country": "United States"
                    }
                  }
                ]
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await OpenStreetMapHelper.BuildAddressLocationsAsync(
                    "1600 Pennsylvania Avenue NW, Washington, DC", "US");

                // Assert
                locations.Should().NotBeNull();
                locations.Should().HaveCount(1);

                var loc = locations![0];
                loc.City.Should().Be("Washington");
                loc.County.Should().Be("District of Columbia");
                loc.State.Should().Be("District of Columbia");
                loc.Zip.Should().Be("20500");
                loc.Country.Should().Be("United States");
                loc.Latitude.Should().Be("38.8977");
                loc.Longitude.Should().Be("-77.0366");

                var expectedKey = SecurityHelper.Base64Encode("38.8977,-77.0366");
                loc.Key.Should().Be(expectedKey);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldUseTown_WhenCityIsAbsent()
        {
            // Arrange — result where address has town but no city
            var json =
                """
                [
                  {
                    "place_id": 9999,
                    "lat": "51.5074",
                    "lon": "-0.1278",
                    "display_name": "London",
                    "addresstype": "city",
                    "importance": 0.95,
                    "address": {
                      "town": "Westminster",
                      "county": "Greater London",
                      "state": "England",
                      "postcode": "SW1A",
                      "country": "United Kingdom"
                    }
                  }
                ]
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await OpenStreetMapHelper.BuildAddressLocationsAsync("Westminster, London", "GB");

                // Assert
                locations.Should().NotBeNull();
                locations![0].City.Should().Be("Westminster");
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldReturnNull_WhenResponseIsEmptyArray()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await OpenStreetMapHelper.BuildAddressLocationsAsync("nonexistent place xyz", "US");

                // Assert
                locations.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldReturnNull_WhenHttpRequestFails()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await OpenStreetMapHelper.BuildAddressLocationsAsync("query", "US");

                // Assert
                locations.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldReturnNull_WhenResponseIsInvalidJson()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await OpenStreetMapHelper.BuildAddressLocationsAsync("query", "US");

                // Assert
                locations.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }
    }
}
