using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Models.Implementation.AirQuality;
using Xcalibur.Weather.Models.Implementation.WeatherForecast;
using Xcalibur.Weather.Models.Services.OpenMeteo.CurrentAirQuality;
using Xcalibur.Weather.Models.Services.OpenMeteo.CurrentWeather;
using Xcalibur.Weather.Models.Services.OpenMeteo.DailyWeather;
using Xcalibur.Weather.Models.Services.OpenMeteo.HourlyWeather;
using Xcalibur.Weather.Services;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for OpenMeteo related operations.
    /// </summary>
    public static class OpenMeteoHelper
    {
        // Shared HttpClient instance for all GeocodioService instances to optimize resource usage.
        private static HttpClient _sharedHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /// <summary>
        /// Builds the air quality point.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<AirQualityPoint?> BuildAirQualityPointAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var airQualityResponse = await GetCurrentAirQualityAsync(latitude, longitude, logger, token);

            // Build AirQualityPoint
            return airQualityResponse?.Current is null ? null : new AirQualityPoint(airQualityResponse.Current);
        }

        /// <summary>
        /// Builds the current forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<DetailedForecastPoint?> BuildCurrentForecastAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var currentWeatherResponse = await GetCurrentWeatherForecastAsync(latitude, longitude, logger, token);

            // Hourly forecast must have a value to scroll.
            return currentWeatherResponse?.Current is not { } data 
                ? null 
                : new DetailedForecastPoint(data);
        }

        /// <summary>
        /// Builds the hourly forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<HourlyForecastPoint[]?> BuildHourlyForecastAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var response = await GetHourlyForecastAsync(latitude, longitude, logger, token);

            // Build hourly forecast points.
            var forecastPoints = InternalBuildHourlyForecast(response);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Builds the yesterday's forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="dateValue">The date value.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<HourlyForecastPoint[]?> BuildYesterdayHourlyForecastAsync(string latitude, string longitude, string dateValue, ILogger logger, CancellationToken token)
        {
            var response = await GetYesterdayHourlyForecastAsync(latitude, longitude, dateValue, logger, token);

            // Build hourly forecast points.
            var forecastPoints = InternalBuildHourlyForecast(response);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Builds the daily forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<DailyForecastPoint[]?> BuildDailyForecastAsync(string latitude, string longitude, int forecastDays, ILogger logger, CancellationToken token)
        {
            var response = await GetDailyForecastAsync(latitude, longitude, forecastDays, logger, token);

            // Build daily forecast points.
            var forecastPoints = InternalBuildDailyForecast(response);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Builds the yesterday daily forecast asynchronous.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="startDateValue">The start date value.</param>
        /// <param name="endDateValue">The end date value.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static async Task<DailyForecastPoint[]?> BuildYesterdayDailyForecastAsync(string latitude, string longitude, string startDateValue, string endDateValue, ILogger logger, CancellationToken token)
        {
            var response = await GetYesterdayDailyForecastAsync(latitude, longitude, startDateValue, endDateValue, logger, token);

            // Build daily forecast points.
            var forecastPoints = InternalBuildDailyForecast(response);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Gets the current air quality reading asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<AirQualityResponse?> GetCurrentAirQualityAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the current weather for specific latitude and longitude.
            return await service.GetCurrentAirQualityAsync(latitude, longitude, token);
        }

        /// <summary>
        /// Gets the current weather forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<CurrentWeatherResponse?> GetCurrentWeatherForecastAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the current weather for specific latitude and longitude.
            return await service.GetCurrentWeatherAsync(latitude, longitude, token);
        }

        /// <summary>
        /// Gets the hourly forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<HourlyWeatherResponse?> GetHourlyForecastAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the hourly weather for specific latitude and longitude.
            return await service.GetHourlyForecastAsync(latitude, longitude, token);
        }

        /// <summary>
        /// Gets the hourly forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="dateValue">The date value.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<HourlyWeatherResponse?> GetYesterdayHourlyForecastAsync(string latitude, string longitude, string dateValue, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get weather for yesterday for specific latitude and longitude.
            return await service.GetYesterdayHourlyForecastAsync(latitude, longitude, dateValue, token);
        }

        /// <summary>
        /// Builds the hourly forecast.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private static HourlyForecastPoint[]? InternalBuildHourlyForecast(HourlyWeatherResponse? response)
        {
            // Hourly forecast must have a value to scroll.
            if (response?.Hourly is not { } data) return null;

            // No precipitation data to build forecast points.
            if (data.Time.Length is 0) return null;

            // Create a string representation of the current hour.
            var nowValue = DateTime.Now.ToString("yyyy-MM-ddTHH:00");

            // Build daily forecast points.
            var forecastPoints = new HourlyForecastPoint[data.Time.Length];
            for (var index = 0; index < data.Time.Length; index++)
            {
                var dateValue = data.Time[index];
                var isCurrent = dateValue == nowValue;

                // Map data to forecast point
                forecastPoints[index] = new HourlyForecastPoint(data, index, isCurrent);
            }

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Gets the daily forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<DailyWeatherResponse?> GetDailyForecastAsync(string latitude, string longitude, int forecastDays, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the daily weather for specific latitude and longitude.
            return await service.GetDailyForecastAsync(latitude, longitude, forecastDays, token);
        }

        /// <summary>
        /// Gets the yesterday daily forecast asynchronous.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="startDateValue">The start date value.</param>
        /// <param name="endDateValue">The end date value.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<DailyWeatherResponse?> GetYesterdayDailyForecastAsync(string latitude, string longitude, string startDateValue, string endDateValue, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the daily weather for specific latitude and longitude.
            return await service.GetYesterdayDailyForecastAsync(latitude, longitude, startDateValue, endDateValue, token);
        }

        /// <summary>
        /// Internals the build daily forecast.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private static DailyForecastPoint[]? InternalBuildDailyForecast(DailyWeatherResponse? response)
        {
            // Hourly forecast must have a value to scroll.
            if (response?.Daily is not { } data) return null;

            // No precipitation data to build forecast points.
            if (data.Time.Length is 0) return null;

            // Create a string representation of the current hour.
            var nowValue = DateTime.Now.ToString("yyyy-MM-dd");

            // Build daily forecast points.
            var forecastPoints = new DailyForecastPoint[data.Time.Length];
            for (var index = 0; index < data.Time.Length; index++)
            {
                forecastPoints[index] = new DailyForecastPoint(data, index, nowValue);
            }

            // Return the built forecast points.
            return forecastPoints;
        }
    }
}
