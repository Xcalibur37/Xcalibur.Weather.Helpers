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
    /// Tests for <see cref="GeocodioHelper"/>.
    /// </summary>
    public sealed class GeocodioHelperTests
    {
        private readonly HttpClient? _originalClient;
        private readonly FieldInfo _sharedClientField;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeocodioHelperTests"/> class.
        /// </summary>
        /// <exception cref="System.Exception">SharedHttpClient field not found</exception>
        public GeocodioHelperTests()
        {
            // Locate private static readonly SharedHttpClient field
            _sharedClientField = typeof(GeocodioHelper).GetField("_sharedHttpClient", BindingFlags.Static | BindingFlags.NonPublic)
                                 ?? throw new Exception("SharedHttpClient field not found");

            // Save original client so we can restore after tests
            _originalClient = (HttpClient?)_sharedClientField.GetValue(null);
        }

        /// <summary>
        /// Replaces the shared HTTP client.
        /// </summary>
        /// <param name="handler">The handler.</param>
        private void ReplaceSharedHttpClient(HttpMessageHandler handler)
        {
            var replacement = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

            // Replace the private static readonly field via reflection
            _sharedClientField.SetValue(null, replacement);
        }

        /// <summary>
        /// Restores the original HTTP client.
        /// </summary>
        private void RestoreOriginalHttpClient()
        {
            _sharedClientField.SetValue(null, _originalClient);
        }

        /// <summary>
        /// Builds the address locations asynchronous should return mapped locations when geocodio returns result.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldReturnMappedLocations_WhenGeocodioReturnsResult()
        {
            // Arrange - craft a valid Geocodio JSON with one result
            var json =
                """
                {
                   "results": [
                       {
                           "address_components": {
                               "city": "TestCity",
                               "county": "TestCounty",
                               "state": "TS",
                               "zip": "99999",
                               "country": "US"
                           },
                           "location": { "lat": 12.34, "lng": 56.78 }
                       }
                   ]
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await GeocodioHelper.BuildAddressLocationsAsync("APIKEY", "query", "US");

                // Assert
                locations.Should().NotBeNull();
                locations.Should().HaveCount(1);

                var loc = locations[0];
                loc.City.Should().Be("TestCity");
                loc.Zip.Should().Be("99999");
                loc.State.Should().Be("TS");
                loc.Country.Should().Be("US");
                loc.Latitude.Should().Be("12.34");
                loc.Longitude.Should().Be("56.78");

                var expectedKey = SecurityHelper.Base64Encode("12.34,56.78");
                loc.Key.Should().Be(expectedKey);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the address locations asynchronous should return null when geocodio response has no results.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BuildAddressLocationsAsync_ShouldReturnNull_WhenGeocodioResponseHasNoResults()
        {
            // Arrange - empty object => Results will be null
            var json = "{}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));

            try
            {
                // Act
                var locations = await GeocodioHelper.BuildAddressLocationsAsync("APIKEY", "query", "US");

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