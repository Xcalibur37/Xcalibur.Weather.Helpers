using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models.WeatherProvider.GooglePollen.Forecast;
using Xcalibur.Weather.Services.WeatherProvider.GooglePollen;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for Google Pollen related operations.
    /// </summary>
    public static class GooglePollenHelper
    {
        /// <summary>
        /// Builds the pollen forecast.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger (optional).</param>
        /// <returns></returns>
        public static async Task<PollenForecastResponse?> BuildPollenForecastAsync(string apiKey, string latitude,
            string longitude, ILogger? logger = null)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return await GetPollenForecastAsync(apiKey, latitude, longitude, logger);
            }
            logger?.LogWarning("Google Pollen API key is null or empty");
            return null;

        }

        /// <summary>
        /// Tests the API key asynchronously.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<bool> TestApiKeyAsync(string apiKey, ILogger? logger)
        {
            var service = CreateService(apiKey, logger);

            // Use a short timeout so the caller is not blocked indefinitely.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Validate the API key against the Google Pollen endpoint.
            return await service.TestApiKey(cts.Token);
        }

        /// <summary>
        /// Gets the pollen forecast data.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<PollenForecastResponse?> GetPollenForecastAsync(string apiKey, string latitude,
            string longitude, ILogger? logger)
        {
            var service = CreateService(apiKey, logger);

            // Use a short timeout so the caller is not blocked indefinitely.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Fetch the pollen forecast for the given coordinates.
            return await service.GetPollenForecastAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static GooglePollenService CreateService(string apiKey, ILogger? logger)
        {
            var http = new HttpClient();
            var serviceLogger = logger != null
                ? new LoggerFactory().CreateLogger<GooglePollenService>()
                : NullLogger<GooglePollenService>.Instance;
            return new GooglePollenService(http, apiKey, serviceLogger);
        }
    }
}
