using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.WeatherProvider.IpGeo.Astronomy;
using Xcalibur.Weather.Services.WeatherProvider.IpGeo;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for IP Geolocation related operations.
    /// </summary>
    public static class IpGeoHelper
    {
        /// <summary>
        /// Builds the sun moon point.
        /// </summary>
        /// <param name="ipGeoApiKey">The ip geo API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger (optional).</param>
        /// <returns></returns>
        public static async Task<SunMoonPoint?> BuildSunMoonPoint(string ipGeoApiKey, string latitude, string longitude,
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(ipGeoApiKey))
            {
                logger?.LogWarning("IpGeo API key is null or empty");
                return null;
            }

            var response = await GetSunMoonDataAsync(ipGeoApiKey, latitude, longitude, logger);

            // Build SunMoonPoint
            return response?.Astronomy is null ? null : new SunMoonPoint(response.Astronomy);
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

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get the sun/moon data for specific latitude and longitude.
            return await service.TestApiKey(cts.Token);
        }

        /// <summary>
        /// Gets the sun and moon data.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<SunMoonDataResponse?> GetSunMoonDataAsync(string apiKey, string latitude,
            string longitude, ILogger? logger)
        {
            var service = CreateService(apiKey, logger);

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get the sun/moon data for specific latitude and longitude.
            return await service.GetSunMoonDataAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static IpGeoService CreateService(string apiKey, ILogger? logger)
        {
            var http = new HttpClient();
            var serviceLogger = logger != null
                ? new LoggerFactory().CreateLogger<IpGeoService>()
                : NullLogger<IpGeoService>.Instance;
            return new IpGeoService(http, apiKey, serviceLogger);
        }
    }
}