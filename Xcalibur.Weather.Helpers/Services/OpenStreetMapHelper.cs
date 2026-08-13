using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models.Implementation.Geocoding;
using Xcalibur.Weather.Models.Services.OpenStreetMap.Response;
using Xcalibur.Weather.Services;

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
        /// <param name="languageCode">The language code.</param>
        /// <param name="country">The country.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>
        /// An array of <see cref="AddressLocationModel" /> entries, or <c>null</c> when no results
        /// are found.
        /// </returns>
        public static async Task<AddressLocationModel[]?> BuildAddressLocationsAsync(
            string query, string? languageCode, string? country, ILogger? logger = null)
        {
            // Validate input parameters.
            var results = await GetLocationsAsync(query, languageCode, country, logger);

            // If no results are found, return null.
            if (results is not { Count: > 0 }) return null;

            // Map the results to AddressLocationModel instances.
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
        /// <param name="languageCode">The language code.</param>
        /// <param name="country">The country.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<List<OpenStreetMapResultResponse>?> GetLocationsAsync(string query, string? languageCode, string? country, ILogger? logger)
        {
            // Create a new service instance for each call to ensure thread safety, but reuse the shared HttpClient.
            var service = CreateService(logger);

            // Use a cancellation token to prevent hanging indefinitely if the service is unresponsive.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await service.GetLocationsAsync(query,  languageCode,country, cts.Token);
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
