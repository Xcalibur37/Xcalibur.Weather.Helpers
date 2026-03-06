using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.WeatherProvider.Geocodio;
using Xcalibur.Weather.Services.WeatherProvider.Geocodio;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for Geocodio service.
    /// </summary>
    public static class GeocodioHelper
    {
        // Shared HttpClient instance for all GeocodioService instances to optimize resource usage.
        private static HttpClient _sharedHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>
        /// Tests the API key asynchronously.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<bool> TestApiKeyAsync(string apiKey, ILogger? logger)
        {
            // Create the Geocodio service instance
            var service = CreateService(apiKey, logger);

            // Set a cancellation token with a 10-second timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await service.TestApiKey(cts.Token);
        }

        /// <summary>
        /// Builds the address locations asynchronous.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="query">The query.</param>
        /// <param name="country">The country.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<AddressLocationModel[]?> BuildAddressLocationsAsync(string apiKey, string query, string country, ILogger? logger = null)
        {
            // Get the locations from Geocodio service
            var response = await GetLocationsAsync(apiKey, query, country, logger);

            // If no results, return null
            if (response?.Results is not { } data) return null;

            // Map the results to AddressLocationModel array
            var locations = new AddressLocationModel[data.Count];
            for (var index = 0; index < data.Count; index++)
            {
                locations[index] = new AddressLocationModel(data[index]);
            }

            // Return the array of locations
            return locations;
        }

        /// <summary>
        /// Gets the locations asynchronous.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="query">The query.</param>
        /// <param name="country">The country.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<GeocodioResponse?> GetLocationsAsync(string apiKey, string query, string country, ILogger? logger)
        {
            // Create the Geocodio service instance
            var service = CreateService(apiKey, logger);

            // Set a cancellation token with a 10-second timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await service.GetLocationsAsync(query, country, cts.Token);
        }

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static GeocodioService CreateService(string apiKey, ILogger? logger)
        {
            var serviceLogger = logger ?? NullLogger<GeocodioService>.Instance;
            return new GeocodioService(_sharedHttpClient, apiKey, serviceLogger);
        }
    }
}