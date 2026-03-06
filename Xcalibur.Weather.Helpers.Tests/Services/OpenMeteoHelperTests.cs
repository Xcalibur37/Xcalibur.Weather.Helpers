using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models.Testing;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    public sealed class OpenMeteoHelperTests
    {
        private readonly FieldInfo _sharedClientField;
        private readonly HttpClient? _originalClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenMeteoHelperTests"/> class.
        /// </summary>
        /// <exception cref="Exception">SharedHttpClient field not found</exception>
        public OpenMeteoHelperTests()
        {
            _sharedClientField = typeof(OpenMeteoHelper).GetField("_sharedHttpClient", BindingFlags.Static | BindingFlags.NonPublic)
                                  ?? throw new Exception("SharedHttpClient field not found");
            _originalClient = (HttpClient?)_sharedClientField.GetValue(null);
        }

        /// <summary>
        /// Replaces the shared HTTP client.
        /// </summary>
        /// <param name="handler">The handler.</param>
        private void ReplaceSharedHttpClient(HttpMessageHandler handler)
        {
            var replacement = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            _sharedClientField.SetValue(null, replacement);
        }

        /// <summary>
        /// Restores the original HTTP client.
        /// </summary>
        private void RestoreOriginalHttpClient()
        {
            _sharedClientField.SetValue(null, _originalClient);
        }

        /// <summary>
        /// Builds the air quality point should map current air quality response.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BuildAirQualityPoint_ShouldMapCurrent_AirQualityResponse()
        {
            // Arrange - a minimal but valid AirQualityResponse JSON
            var json =
                """
                {
                  "latitude": 12.34,
                  "longitude": 56.78,
                  "current": {
                    "time": "2023-01-01T12:00",
                    "interval": 1,
                    "us_aqi": 75,
                    "pm10": 1.2,
                    "carbon_monoxide": 0.3,
                    "pm2_5": 2.1,
                    "nitrogen_dioxide": 0.1,
                    "sulphur_dioxide": 0.0,
                    "ozone": 0.05
                  }
                }
                """;

            // Act - setup response
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Replace shared HTTP client with stub
            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                // Act
                var logger = NullLogger.Instance;
                var point = await OpenMeteoHelper.BuildAirQualityPoint("12.34", "56.78", logger);

                // Assert
                point.Should().NotBeNull();
                point.UsAqi.Should().Be(75);
                point.UsAqiValue.Should().Be("Moderate"); // 75 -> Moderate per helper mapping
                point.Pm25.Should().BeApproximately(2.1, 0.0001);
                point.CarbonMonoxide.Should().BeApproximately(0.3, 0.0001);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the daily forecast should return daily forecast points when daily response present.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BuildDailyForecast_ShouldReturnDailyForecastPoints_WhenDailyResponsePresent()
        {
            // Arrange - small daily response with two days
            var json =
                """
                {
                  "daily": {
                    "time": ["2023-01-01", "2023-01-02"],
                    "weather_code": [0, 1],
                    "temperature_2m_max": [10.0, 12.0],
                    "temperature_2m_min": [1.0, 2.0],
                    "sunrise": ["06:00", "06:01"],
                    "sunset": ["18:00", "18:01"],
                    "daylight_duration": [43200, 43200],
                    "sunshine_duration": [3600, 3600],
                    "rain_sum": [0.0, 0.5],
                    "showers_sum": [0.0, 0.1],
                    "snowfall_sum": [0.0, 0.0],
                    "precipitation_sum": [0.0, 0.5],
                    "precipitation_hours": [0.0, 1.0],
                    "precipitation_probability_max": [0.0, 10.0],
                    "wind_speed_10m_max": [5.0, 6.0],
                    "wind_gusts_10m_max": [7.0, 8.0],
                    "uv_index_max": [1.0, 2.0]
                  }
                }
                """;

            // Setup response
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Replace shared HTTP client with stub
            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                // Act
                var logger = NullLogger.Instance;
                var points = await OpenMeteoHelper.BuildDailyForecast("12.34", "56.78", 2, logger);

                // Assert
                points.Should().NotBeNull();
                points.Should().HaveCount(2);
                points[0].DateValue.Should().Be("2023-01-01");
                points[0].HighTemp.Should().BeApproximately(10.0, 0.0001);
                points[0].LowTemp.Should().BeApproximately(1.0, 0.0001);

                points[1].DateValue.Should().Be("2023-01-02");
                points[1].HighTemp.Should().BeApproximately(12.0, 0.0001);
                points[1].LowTemp.Should().BeApproximately(2.0, 0.0001);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly forecast should map hourly points and mark current.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BuildHourlyForecast_ShouldMapHourlyPoints_AndMarkCurrent()
        {
            // Arrange - small hourly response with two hours
            var nowValue = DateTime.Now.ToString("yyyy-MM-ddTHH:00");
            var laterValue = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:00");

            // Build JSON programmatically to avoid brace-escaping issues
            var hourlyObj = new
            {
                hourly = new
                {
                    time = new[] { nowValue, laterValue },
                    weather_code = new[] { 0, 1 },
                    temperature_2m = new[] { 15.0, 16.0 },
                    apparent_temperature = new[] { 15.0, 16.0 },
                    relative_humidity_2m = new[] { 50.0, 51.0 },
                    dew_point_2m = new[] { 5.0, 5.5 },
                    precipitation_probability = new[] { 0.0, 10.0 },
                    precipitation = new[] { 0.0, 0.1 },
                    rain = new[] { 0.0, 0.0 },
                    showers = new[] { 0.0, 0.0 },
                    snowfall = new[] { 0.0, 0.0 },
                    snow_depth = new[] { 0.0, 0.0 },
                    pressure_msl = new[] { 1013.0, 1012.5 },
                    surface_pressure = new[] { 1015.0, 1014.5 },
                    cloud_cover = new[] { 10.0, 20.0 },
                    visibility = new[] { 10000, 10000 },
                    wind_speed_10m = new[] { 3.0, 4.0 },
                    wind_direction_10m = new[] { 180, 190 },
                    wind_gusts_10m = new[] { 5.0, 6.0 }
                }
            };

            var json = JsonSerializer.Serialize(hourlyObj);

            // Setup response
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Replace shared HTTP client with stub
            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                // Act
                var logger = NullLogger.Instance;
                // choose sunrise/sunset to make current point be daytime
                var sunrise = new TimeOnly(6, 0);
                var sunset = new TimeOnly(22, 0);
                var points = await OpenMeteoHelper.BuildHourlyForecast("12.34", "56.78", true, sunrise, sunset, logger);

                // Assert
                points.Should().NotBeNull();
                points.Should().HaveCount(2);

                var first = points[0];
                first.DateValue.Should().Be(nowValue);
                first.Temperature.Should().BeApproximately(15.0, 0.0001);
                first.IsCurrent.Should().BeTrue();
                
                var second = points[1];
                second.DateValue.Should().Be(laterValue);
                second.Temperature.Should().BeApproximately(16.0, 0.0001);
                second.IsCurrent.Should().BeFalse();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }
    }
}