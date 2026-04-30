using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Services.WeatherProvider.SunriseSunset;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for SunriseSunset.io API operations.
    /// No API key is required; coordinates are the only input.
    /// </summary>
    public static class SunriseSunsetHelper
    {
        /// <summary>
        /// Fetches sun and moon data from SunriseSunset.io and maps the result
        /// to a <see cref="SunMoonPoint"/>.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger (optional).</param>
        /// <returns>
        /// A populated <see cref="SunMoonPoint"/> on success; <c>null</c> on failure
        /// or when the API does not return a usable result.
        /// </returns>
        public static async Task<SunMoonPoint?> BuildSunMoonPoint(string latitude, string longitude,
            ILogger? logger = null)
        {
            var response = await GetSunriseSunsetAsync(latitude, longitude, logger);
            if (response?.Results is null) return null;

            return new SunMoonPoint(response.Results);
        }

        /// <summary>
        /// Calls the SunriseSunset.io API and returns the raw response.
        /// </summary>
        private static async Task<Models.WeatherProvider.SunriseSunset.SunriseSunsetResponse?> GetSunriseSunsetAsync(
            string latitude, string longitude, ILogger? logger)
        {
            var service = CreateService(logger);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await service.GetSunriseSunsetAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Creates a configured <see cref="SunriseSunsetService"/> instance.
        /// </summary>
        private static SunriseSunsetService CreateService(ILogger? logger)
        {
            var http = new HttpClient();
            var serviceLogger = logger != null
                ? new LoggerFactory().CreateLogger<SunriseSunsetService>()
                : NullLogger<SunriseSunsetService>.Instance;
            return new SunriseSunsetService(http, serviceLogger);
        }
    }
}
