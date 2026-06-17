using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models.Implementation.Pollen;
using Xcalibur.Weather.Models.Services.Atmospore.Response;
using Xcalibur.Weather.Services;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for Atmospore pollen related operations.
    /// </summary>
    public static class AtmosporeHelper
    {
        /// <summary>
        /// Builds the pollen forecast from the Atmospore API.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="date">The forecast date (yyyy-MM-dd). Defaults to today if null or empty.</param>
        /// <param name="forecastDays">The number of forecast days.</param>
        /// <param name="logger">The logger (optional).</param>
        /// <returns></returns>
        public static async Task<PollenInformation?> BuildPollenForecastAsync(string apiKey, string latitude,
            string longitude, string? date = null, int forecastDays = 1, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger?.LogWarning("Atmospore API key is null or empty");
                return null;
            }

            // If date is null or empty, the API will default to today's date, so we can pass it as is.
            var response = await GetPollenForecastAsync(apiKey, latitude, longitude, date, forecastDays, logger);

            // If the response is null, it means there was an error or no data, so we return null.
            return response is null ? null : new PollenInformation(response);
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

            // The TestApiKey method will return true if the API key is valid, false otherwise.
            return await service.TestApiKey(cts.Token);
        }

        /// <summary>
        /// Gets the pollen forecast data from the Atmospore API.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="date">The forecast date (yyyy-MM-dd). Defaults to today if null or empty.</param>
        /// <param name="forecastDays">The number of forecast days.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<PollenResponse?> GetPollenForecastAsync(string apiKey, string latitude,
            string longitude, string? date, int forecastDays, ILogger? logger)
        {
            var service = CreateService(apiKey, logger);

            // Use a short timeout so the caller is not blocked indefinitely.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // The GetPollenForecastAsync method will return the pollen forecast data or null if there was an error.
            return await service.GetPollenForecastAsync(latitude, longitude, date, forecastDays, cts.Token);
        }

        /// <summary>
        /// Creates the Atmospore service.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static AtmosporeService CreateService(string apiKey, ILogger? logger)
        {
            var http = new HttpClient();
            var serviceLogger = logger != null
                ? new LoggerFactory().CreateLogger<AtmosporeService>()
                : NullLogger<AtmosporeService>.Instance;
            return new AtmosporeService(http, apiKey, serviceLogger);
        }
    }
}
