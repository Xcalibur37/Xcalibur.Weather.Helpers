using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Models.Implementation.AirQuality;
using Xcalibur.Weather.Models.Implementation.WeatherForecast;
using Xcalibur.Weather.Models.Services.OpenMeteo.CurrentAirQuality;
using Xcalibur.Weather.Models.Services.OpenMeteo.CurrentWeather;
using Xcalibur.Weather.Models.Services.OpenMeteo.DailyWeather;
using Xcalibur.Weather.Models.Services.OpenMeteo.HourlyAirQuality;
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

        #region Current Forecast

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
            return await service.GetCurrentWeatherAsync(latitude, longitude, "", token);
        }

        #endregion

        #region Hourly Forecast

        /// <summary>
        /// Builds the hourly forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<HourlyForecastPoint[]?> BuildHourlyForecastAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var response = await GetHourlyForecastAsync(latitude, longitude, forecastDays, pastDays, logger, token);
            var supplementalResponse = await GetHourlyForecastSupplementalAsync(latitude, longitude, forecastDays, pastDays, logger, token);

            // Build hourly forecast points.
            var forecastPoints = InternalBuildHourlyForecast(response, supplementalResponse);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Gets the hourly forecast for the next 48 hours asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<HourlyWeatherResponse?> GetHourlyForecastAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the hourly weather for specific latitude and longitude.
            return await service.GetHourlyForecastAsync(latitude, longitude, forecastDays, pastDays, "", token);
        }

        /// <summary>
        /// Gets the supplemental hourly forecast for the next 48 hours asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<HourlyWeatherResponse?> GetHourlyForecastSupplementalAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the supplemental hourly weather for specific latitude and longitude.
            return await service.GetHourlyForecastSupplementalAsync(latitude, longitude, forecastDays, pastDays, token);
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
        /// <param name="supplementalResponse">The supplemental response.</param>
        /// <returns></returns>
        private static HourlyForecastPoint[]? InternalBuildHourlyForecast(HourlyWeatherResponse? response, HourlyWeatherResponse? supplementalResponse = null)
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
                var point = new HourlyForecastPoint(data, index, isCurrent);
                if (supplementalResponse?.Hourly is { } supplementalData && supplementalData.Time.Length > index)
                {
                    // Map supplemental data to forecast point
                    point.Map(supplementalData, index, isCurrent);
                }

                // Assign the point to the forecast points array.
                forecastPoints[index] = point;
            }

            // Return the built forecast points.
            return forecastPoints;
        }

        #endregion

        #region Daily Forecast

        /// <summary>
        /// Builds the daily forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<DailyForecastPoint[]?> BuildDailyForecastAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var response = await GetDailyForecastAsync(latitude, longitude, forecastDays, pastDays, logger, token);
            var supplementalResponse = await GetDailyForecastSupplementalAsync(latitude, longitude, forecastDays, pastDays, logger, token);

            // Build daily forecast points.
            var forecastPoints = InternalBuildDailyForecast(response, supplementalResponse);

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
        /// Gets the daily forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<DailyWeatherResponse?> GetDailyForecastAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the daily weather for specific latitude and longitude.
            return await service.GetDailyForecastAsync(latitude, longitude, forecastDays, pastDays, "", token);
        }

        /// <summary>
        /// Gets the daily forecast supplemental asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<DailyWeatherResponse?> GetDailyForecastSupplementalAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the daily weather for specific latitude and longitude.
            return await service.GetDailyForecastSupplementalAsync(latitude, longitude, forecastDays, pastDays, token);
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
        /// Builds the daily forecast.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="supplementalResponse">The supplemental response.</param>
        /// <returns></returns>
        private static DailyForecastPoint[]? InternalBuildDailyForecast(DailyWeatherResponse? response, DailyWeatherResponse? supplementalResponse = null)
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
                // Map data to forecast point
                var point = new DailyForecastPoint(data, index, nowValue);
                if (supplementalResponse?.Daily is { } supplementalData && supplementalData.Time.Length > index)
                {
                    // Map supplemental data to forecast point
                    point.Map(supplementalData, index, nowValue);
                }

                // Assign the point to the forecast points array.
                forecastPoints[index] = point;
            }

            // Return the built forecast points.
            return forecastPoints;
        }

        #endregion

        #region Air Quality Forecast

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
        /// Gets the current air quality reading asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<CurrentAirQualityResponse?> GetCurrentAirQualityAsync(string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the current weather for specific latitude and longitude.
            return await service.GetCurrentAirQualityAsync(latitude, longitude, token);
        }

        /// <summary>
        /// Builds the hourly air quality forecast points.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static async Task<AirQualityPoint[]?> BuildHourlyAirQualityAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var response = await GetHourlyAirQualityAsync(latitude, longitude, forecastDays, pastDays, logger, token);

            // Hourly forecast must have a value to scroll.
            if (response?.Hourly is not { } data) return null;

            // No precipitation data to build forecast points.
            if (data.Time.Length is 0) return null;

            // Create a string representation of the current hour.
            var nowValue = DateTime.Now.ToString("yyyy-MM-ddTHH:00");

            // Build daily forecast points.
            var points = new AirQualityPoint[data.Time.Length];
            for (var index = 0; index < data.Time.Length; index++)
            {
                var dateValue = data.Time[index];
                var isCurrent = dateValue == nowValue;

                // Map data to forecast point
                points[index] = new AirQualityPoint(data, index, isCurrent);
            }

            // Return the built forecast points.
            return points;
        }

        /// <summary>
        /// Gets the hourly air quality forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="pastDays">The past days.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<HourlyAirQualityResponse?> GetHourlyAirQualityAsync(string latitude, string longitude, int forecastDays, int pastDays, ILogger logger, CancellationToken token)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Get the current weather for specific latitude and longitude.
            return await service.GetHourlyAirQualityAsync(latitude, longitude, forecastDays, pastDays, token);
        }

        #endregion
    }
}
