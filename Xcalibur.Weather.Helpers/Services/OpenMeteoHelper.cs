using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Models;
using Xcalibur.Weather.Models.WeatherProvider.OpenMeteo.CurrentAirQuality;
using Xcalibur.Weather.Models.WeatherProvider.OpenMeteo.CurrentWeather;
using Xcalibur.Weather.Models.WeatherProvider.OpenMeteo.DailyWeather;
using Xcalibur.Weather.Models.WeatherProvider.OpenMeteo.HourlyWeather;
using Xcalibur.Weather.Services.WeatherProvider.OpenMeteo;

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
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>
        /// Builds the air quality point.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<AirQualityPoint?> BuildAirQualityPoint(string latitude, string longitude, ILogger logger)
        {
            var airQualityResponse = await GetCurrentAirQualityAsync(latitude, longitude, logger);

            // Build AirQualityPoint
            return airQualityResponse?.Current is null ? null : new AirQualityPoint(airQualityResponse.Current);
        }

        /// <summary>
        /// Builds the current forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="canAssessDayNight">if set to <c>true</c> [can assess day night].</param>
        /// <param name="sunrise">The sunrise.</param>
        /// <param name="sunset">The sunset.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<CurrentForecastPoint?> BuildCurrentForecast(string latitude, string longitude,
            bool canAssessDayNight, TimeOnly? sunrise, TimeOnly? sunset, ILogger logger)
        {
            var currentWeatherResponse = await GetCurrentWeatherForecastAsync(latitude, longitude, logger);

            // Hourly forecast must have a value to scroll.
            if (currentWeatherResponse?.Current is not { } data) return null;

            // Get time value and parse to local date.
            var dateValue = data.Time;
            var localDate = DateTime.Parse(dateValue!);

            // Assess day or night.
            var isDayTime = true;
            if (sunrise is not null && sunset is not null)
            {
                isDayTime = canAssessDayNight &&
                    localDate.TimeOfDay >= sunrise.Value.ToTimeSpan() &&
                    localDate.TimeOfDay < sunset.Value.ToTimeSpan();
            }
            
            // Map data to forecast point
            return new CurrentForecastPoint(data, isDayTime);
        }

        /// <summary>
        /// Builds the daily forecast.
        /// </summary>
        public static async Task<DailyForecastPoint[]?> BuildDailyForecast(string latitude, string longitude, int forecastDays, ILogger logger)
        {
            var dailyWeatherResponse = await GetDailyForecastAsync(latitude, longitude, forecastDays, logger);

            // Hourly forecast must have a value to scroll.
            if (dailyWeatherResponse?.Daily is not { } data) return null;

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

        /// <summary>
        /// Builds the hourly forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="canAssessDayNight">if set to <c>true</c> [can assess day night].</param>
        /// <param name="sunrise">The sunrise.</param>
        /// <param name="sunset">The sunset.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<HourlyForecastPoint[]?> BuildHourlyForecast(string latitude, string longitude,
            bool canAssessDayNight, TimeOnly? sunrise, TimeOnly? sunset, ILogger logger)
        {
            var response = await GetHourlyForecastAsync(latitude, longitude, logger);
            var forecastPoints = InternalBuildHourlyForecast(response, canAssessDayNight, sunrise, sunset);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Builds the yesterday's forecast.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="canAssessDayNight">if set to <c>true</c> [can assess day night].</param>
        /// <param name="sunrise">The sunrise.</param>
        /// <param name="sunset">The sunset.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public static async Task<HourlyForecastPoint[]?> BuildYesterdaysForecast(string latitude, string longitude,
            bool canAssessDayNight, TimeOnly? sunrise, TimeOnly? sunset, ILogger logger)
        {
            var response = await GetYesterdayForecastAsync(latitude, longitude, logger);
            var forecastPoints = InternalBuildHourlyForecast(response, canAssessDayNight, sunrise, sunset);

            // Return the built forecast points.
            return forecastPoints;
        }

        /// <summary>
        /// Gets the current air quality reading asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<AirQualityResponse?> GetCurrentAirQualityAsync(string latitude, string longitude, ILogger logger)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get the current weather for specific latitude and longitude.
            return await service.GetCurrentAirQualityAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Gets the current weather forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<CurrentWeatherResponse?> GetCurrentWeatherForecastAsync(string latitude, string longitude, ILogger logger)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get the current weather for specific latitude and longitude.
            return await service.GetCurrentWeatherAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Gets the daily forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="forecastDays">The forecast days.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<DailyWeatherResponse?> GetDailyForecastAsync(string latitude, string longitude, int forecastDays, ILogger logger)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get the daily weather for specific latitude and longitude.
            return await service.GetDailyForecastAsync(latitude, longitude, forecastDays, cts.Token);
        }

        /// <summary>
        /// Gets the hourly forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<HourlyWeatherResponse?> GetHourlyForecastAsync(string latitude, string longitude, ILogger logger)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get the hourly weather for specific latitude and longitude.
            return await service.GetHourlyForecastAsync(latitude, longitude, cts.Token);
        }

        /// <summary>
        /// Gets the hourly forecast asynchronously.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        private static async Task<HourlyWeatherResponse?> GetYesterdayForecastAsync(string latitude, string longitude, ILogger logger)
        {
            var service = new OpenMeteoService(_sharedHttpClient, logger);

            // Pass coordinates as you prefer (this depends on your service implementation).
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Get weather for yesterday for specific latitude and longitude.
            var dateValue = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            return await service.GetYesterdayForecastAsync(latitude, longitude, dateValue, cts.Token);
        }

        /// <summary>
        /// Builds the hourly forecast.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="canAssessDayNight">if set to <c>true</c> [can assess day night].</param>
        /// <param name="sunrise">The sunrise.</param>
        /// <param name="sunset">The sunset.</param>
        /// <returns></returns>
        private static HourlyForecastPoint[]? InternalBuildHourlyForecast(HourlyWeatherResponse? response, bool canAssessDayNight, 
            TimeOnly? sunrise, TimeOnly? sunset)
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
                var localDate = DateTime.Parse(dateValue!);
                var isCurrent = dateValue == nowValue;

                var isDayTime = true;
                if (sunrise is not null && sunset is not null)
                {
                    isDayTime = canAssessDayNight &&
                        localDate.TimeOfDay >= sunrise.Value.ToTimeSpan() &&
                        localDate.TimeOfDay < sunset.Value.ToTimeSpan();
                }

                // Map data to forecast point
                forecastPoints[index] = new HourlyForecastPoint(data, index, isDayTime, isCurrent);
            }

            // Return the built forecast points.
            return forecastPoints;
        }
    }
}
