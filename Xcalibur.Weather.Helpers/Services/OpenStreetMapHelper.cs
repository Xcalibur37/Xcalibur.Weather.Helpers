using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.WeatherProvider.OpenStreetMap;
using Xcalibur.Weather.Services.WeatherProvider.OpenStreetMap;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for the OpenStreetMap Nominatim geocoding service.
    /// No API key is required.
    /// </summary>
    public static class OpenStreetMapHelper
    {
        // Shared HttpClient instance across all OpenStreetMapService calls.
        private static HttpClient _sharedHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>
        /// Searches for address locations matching <paramref name="query" /> and returns them
        /// mapped to <see cref="AddressLocationModel" />.
        /// </summary>
        /// <param name="query">Free-form address or place query.</param>
        /// <param name="country">The country.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>
        /// An array of <see cref="AddressLocationModel" /> entries, or <c>null</c> when no results
        /// are found.
        /// </returns>
        public static async Task<AddressLocationModel[]?> BuildAddressLocationsAsync(
            string query, string country, ILogger? logger = null)
        {
            var results = await GetLocationsAsync(query, country, logger);
            if (results is not { Count: > 0 }) return null;

            var locations = new AddressLocationModel[results.Count];
            for (var i = 0; i < results.Count; i++)
            {
                locations[i] = new AddressLocationModel(results[i]);
            }

            return locations;
        }

        /// <summary>
        /// Gets the locations asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="country">The country.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<List<OpenStreetMapResult>?> GetLocationsAsync(string query, string country, ILogger? logger)
        {
            // Create a new service instance for each call to ensure thread safety, but reuse the shared HttpClient.
            var service = CreateService(logger);

            // Use a cancellation token to prevent hanging indefinitely if the service is unresponsive.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await service.GetLocationsAsync(query, country, cts.Token);
        }

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static OpenStreetMapService CreateService(ILogger? logger)
        {
            var serviceLogger = logger ?? NullLogger<OpenStreetMapService>.Instance;
            return new OpenStreetMapService(_sharedHttpClient, serviceLogger);
        }
    }
}
