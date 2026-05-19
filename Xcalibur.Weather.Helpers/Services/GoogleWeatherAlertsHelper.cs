using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.WeatherProvider.GoogleWeatherAlerts;
using Xcalibur.Weather.Services.WeatherProvider.GoogleWeatherAlerts;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for Google Weather Alerts related operations.
    /// </summary>
    public static class GoogleWeatherAlertsHelper
    {
        /// <summary>
        /// Builds the weather alert information.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger (optional).</param>
        /// <returns></returns>
        public static async Task<WeatherAlertInformation?> BuildWeatherAlertsAsync(string apiKey, string latitude,
            string longitude, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger?.LogWarning("Google API key is null or empty");
                return null;
            }

            var response = await GetWeatherAlertsAsync(apiKey, latitude, longitude, logger);

            return response is null ? null : new WeatherAlertInformation(response);
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

            // Validate the API key against the Google Weather Alerts endpoint.
            return await service.TestApiKey(cts.Token);
        }

        /// <summary>
        /// Gets the weather alerts data.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<WeatherAlertsResponse?> GetWeatherAlertsAsync(string apiKey, string latitude,
            string longitude, ILogger? logger)
        {
            var service = CreateService(apiKey, logger);

            // Use a short timeout so the caller is not blocked indefinitely.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Fetch the weather alerts for the given coordinates.
            return await service.GetWeatherAlertsAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static GoogleWeatherAlertsService CreateService(string apiKey, ILogger? logger)
        {
            var http = new HttpClient();
            var serviceLogger = logger != null
                ? new LoggerFactory().CreateLogger<GoogleWeatherAlertsService>()
                : NullLogger<GoogleWeatherAlertsService>.Instance;
            return new GoogleWeatherAlertsService(http, apiKey, serviceLogger);
        }
    }
}
