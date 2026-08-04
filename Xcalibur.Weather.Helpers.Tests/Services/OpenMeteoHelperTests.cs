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
                var point = await OpenMeteoHelper.BuildAirQualityPointAsync("12.34", "56.78", logger, CancellationToken.None);

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
                var points = await OpenMeteoHelper.BuildDailyForecastAsync("12.34", "56.78", 2, logger, CancellationToken.None);

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
        /// Builds the current forecast should return current forecast point when current data present.
        /// </summary>
        [Fact]
        public async Task BuildCurrentForecast_ShouldReturnDetailedForecastPoint_WhenCurrentDataPresent()
        {
            // Arrange — time set to "now" so day/night assessment picks daytime
            var timeValue = DateTime.Now.ToString("yyyy-MM-ddTHH:00");
            var json =
                $$$"""
                {
                  "current": {
                    "time": "{{{timeValue}}}",
                    "interval": 900,
                    "temperature_2m": 22.5,
                    "relative_humidity_2m": 60,
                    "apparent_temperature": 21.0,
                    "precipitation": 0.0,
                    "rain": 0.0,
                    "showers": 0.0,
                    "snowfall": 0.0,
                    "weather_code": 1,
                    "cloud_cover": 25,
                    "pressure_msl": 1015.0,
                    "surface_pressure": 1013.0,
                    "wind_speed_10m": 12.0,
                    "wind_direction_10m": 270,
                    "wind_gusts_10m": 18.0
                  }
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;
                var sunrise = new TimeOnly(6, 0);
                var sunset = new TimeOnly(22, 0);

                // Act
                var point = await OpenMeteoHelper.BuildCurrentForecastAsync("12.34", "56.78", logger, CancellationToken.None);

                // Assert
                point.Should().NotBeNull();
                point!.Temperature.Should().BeApproximately(22.5, 0.0001);
                point.RelativeHumidity.Should().BeApproximately(60.0, 0.0001);
                point.ApparentTemperature.Should().BeApproximately(21.0, 0.0001);
                point.WindSpeed.Should().BeApproximately(12.0, 0.0001);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the current forecast should return null when current data is absent.
        /// </summary>
        [Fact]
        public async Task BuildCurrentForecast_ShouldReturnNull_WhenCurrentDataAbsent()
        {
            // Arrange — response with no "current" block
            var json = """{}""";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var point = await OpenMeteoHelper.BuildCurrentForecastAsync("0", "0", logger, CancellationToken.None);

                // Assert
                point.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds yesterday's forecast should return hourly forecast points when data present.
        /// </summary>
        [Fact]
        public async Task BuildYesterdaysForecast_ShouldReturnHourlyForecastPoints_WhenDataPresent()
        {
            // Arrange — two hourly entries for yesterday
            var yesterday = DateTime.Now.AddDays(-1);
            var hour0 = yesterday.Date.ToString("yyyy-MM-ddTHH:00");
            var hour1 = yesterday.Date.AddHours(1).ToString("yyyy-MM-ddTHH:00");

            var hourlyObj = new
            {
                hourly = new
                {
                    time = new[] { hour0, hour1 },
                    weather_code = new[] { 0, 1 },
                    temperature_2m = new[] { 10.0, 11.0 },
                    apparent_temperature = new[] { 9.0, 10.0 },
                    relative_humidity_2m = new[] { 70.0, 71.0 },
                    dew_point_2m = new[] { 4.0, 4.5 },
                    precipitation_probability = new[] { 5.0, 10.0 },
                    precipitation = new[] { 0.0, 0.1 },
                    rain = new[] { 0.0, 0.0 },
                    showers = new[] { 0.0, 0.0 },
                    snowfall = new[] { 0.0, 0.0 },
                    snow_depth = new[] { 0.0, 0.0 },
                    pressure_msl = new[] { 1012.0, 1011.5 },
                    surface_pressure = new[] { 1014.0, 1013.5 },
                    cloud_cover = new[] { 30.0, 40.0 },
                    visibility = new[] { 8000, 9000 },
                    wind_speed_10m = new[] { 5.0, 6.0 },
                    wind_direction_10m = new[] { 90, 100 },
                    wind_gusts_10m = new[] { 8.0, 9.0 }
                }
            };

            var json = JsonSerializer.Serialize(hourlyObj);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;
                var sunrise = new TimeOnly(6, 0);
                var sunset = new TimeOnly(20, 0);

                // Act
                var points = await OpenMeteoHelper.BuildYesterdayHourlyForecastAsync("12.34", "56.78", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), logger, CancellationToken.None);

                // Assert
                points.Should().NotBeNull();
                points.Should().HaveCount(2);
                points![0].DateValue.Should().Be(hour0);
                points[0].Temperature.Should().BeApproximately(10.0, 0.0001);
                points[1].DateValue.Should().Be(hour1);
                points[1].Temperature.Should().BeApproximately(11.0, 0.0001);
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
                var points = await OpenMeteoHelper.BuildHourlyForecastAsync("12.34", "56.78", logger, CancellationToken.None);

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
            /// <summary>
            /// Builds the air quality point returns null when current is absent.
            /// </summary>
            [Fact]
            public async Task BuildAirQualityPoint_ShouldReturnNull_WhenCurrentIsAbsent()
            {
                // Arrange — response with no "current" block
                var json = """{}""";
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
                try
                {
                    var logger = NullLogger.Instance;
                    var point = await OpenMeteoHelper.BuildAirQualityPointAsync("0", "0", logger, CancellationToken.None);

                    point.Should().BeNull();
                }
                finally
                {
                    RestoreOriginalHttpClient();
                }
            }

            /// <summary>
            /// Builds the daily forecast returns null when daily block is absent.
            /// </summary>
            [Fact]
            public async Task BuildDailyForecast_ShouldReturnNull_WhenDailyBlockAbsent()
            {
                // Arrange — response with no "daily" block
                var json = """{}""";
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
                try
                {
                    var logger = NullLogger.Instance;
                    var points = await OpenMeteoHelper.BuildDailyForecastAsync("0", "0", 1, logger, CancellationToken.None);

                    points.Should().BeNull();
                }
                finally
                {
                    RestoreOriginalHttpClient();
                }
            }

            /// <summary>
            /// Builds the daily forecast returns null when time array is empty.
            /// </summary>
            [Fact]
            public async Task BuildDailyForecast_ShouldReturnNull_WhenTimeArrayIsEmpty()
            {
                // Arrange — daily block present but zero time entries
                var json =
                    """
                    {
                      "daily": {
                        "time": [],
                        "weather_code": [],
                        "temperature_2m_max": [],
                        "temperature_2m_min": [],
                        "sunrise": [],
                        "sunset": [],
                        "daylight_duration": [],
                        "sunshine_duration": [],
                        "rain_sum": [],
                        "showers_sum": [],
                        "snowfall_sum": [],
                        "precipitation_sum": [],
                        "precipitation_hours": [],
                        "precipitation_probability_max": [],
                        "wind_speed_10m_max": [],
                        "wind_gusts_10m_max": [],
                        "uv_index_max": []
                      }
                    }
                    """;

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
                try
                {
                    var logger = NullLogger.Instance;
                    var points = await OpenMeteoHelper.BuildDailyForecastAsync("0", "0", 0, logger, CancellationToken.None);

                    points.Should().BeNull();
                }
                finally
                {
                    RestoreOriginalHttpClient();
                }
            }

            /// <summary>
            /// Builds the hourly forecast returns null when hourly block is absent.
            /// </summary>
            [Fact]
            public async Task BuildHourlyForecast_ShouldReturnNull_WhenHourlyBlockAbsent()
            {
                // Arrange
                var json = """{}""";
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
                try
                {
                    var logger = NullLogger.Instance;
                    var points = await OpenMeteoHelper.BuildHourlyForecastAsync("0", "0", logger, CancellationToken.None);

                    points.Should().BeNull();
                }
                finally
                {
                    RestoreOriginalHttpClient();
                }
            }

            /// <summary>
            /// Builds the current forecast marks point as night when time falls outside sunrise/sunset window.
            /// </summary>
            [Fact]
            public async Task BuildCurrentForecast_ShouldMarkNight_WhenTimeIsOutsideSunriseSunset()
            {
                // Arrange — time set to midnight (outside 06:00–22:00 window)
                var timeValue = DateTime.Today.ToString("yyyy-MM-dd") + "T00:00";
                var json =
                    $$$"""
                    {
                      "current": {
                        "time": "{{{timeValue}}}",
                        "interval": 900,
                        "temperature_2m": 10.0,
                        "relative_humidity_2m": 80,
                        "apparent_temperature": 8.0,
                        "precipitation": 0.0,
                        "rain": 0.0,
                        "showers": 0.0,
                        "snowfall": 0.0,
                        "weather_code": 0,
                        "cloud_cover": 0,
                        "pressure_msl": 1015.0,
                        "surface_pressure": 1013.0,
                        "wind_speed_10m": 5.0,
                        "wind_direction_10m": 90,
                        "wind_gusts_10m": 10.0
                      }
                    }
                    """;

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
                try
                {
                    var logger = NullLogger.Instance;
                    var sunrise = new TimeOnly(6, 0);
                    var sunset = new TimeOnly(22, 0);

                    var point = await OpenMeteoHelper.BuildCurrentForecastAsync("0", "0", logger, CancellationToken.None);

                    point.Should().NotBeNull();
                    point!.IsDayTime.Should().BeFalse();
                }
                finally
                {
                    RestoreOriginalHttpClient();
                }
            }

        #region BuildHourlyAirQualityAsync Tests

        /// <summary>
        /// Builds the hourly air quality should return air quality points when data present.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldReturnAirQualityPoints_WhenDataPresent()
        {
            // Arrange - hourly air quality response with two hours
            var hour0 = DateTime.Now.ToString("yyyy-MM-ddTHH:00");
            var hour1 = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:00");

            var airQualityObj = new
            {
                hourly = new
                {
                    time = new[] { hour0, hour1 },
                    pm10 = new[] { 15.5, 16.2 },
                    pm2_5 = new[] { 8.3, 9.1 },
                    carbon_monoxide = new[] { 0.3, 0.35 },
                    nitrogen_dioxide = new[] { 12.5, 13.0 },
                    sulphur_dioxide = new[] { 2.1, 2.3 },
                    ozone = new[] { 45.0, 46.5 },
                    us_aqi = new[] { 42, 45 },
                    european_aqi = new[] { 38, 40 }
                }
            };

            var json = JsonSerializer.Serialize(airQualityObj);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("12.34", "56.78", 2, logger, CancellationToken.None);

                // Assert
                points.Should().NotBeNull();
                points.Should().HaveCount(2);

                // Verify first point
                points![0].Time.Should().Be(hour0);
                points[0].IsCurrent.Should().BeTrue();
                points[0].Pm10.Should().BeApproximately(15.5, 0.0001);
                points[0].Pm25.Should().BeApproximately(8.3, 0.0001);
                points[0].CarbonMonoxide.Should().BeApproximately(0.3, 0.0001);
                points[0].NitrogenDioxide.Should().BeApproximately(12.5, 0.0001);
                points[0].SulphurDioxide.Should().BeApproximately(2.1, 0.0001);
                points[0].Ozone.Should().BeApproximately(45.0, 0.0001);
                points[0].UsAqi.Should().Be(42);
                points[0].EuAqi.Should().Be(38);

                // Verify second point
                points[1].Time.Should().Be(hour1);
                points[1].IsCurrent.Should().BeFalse();
                points[1].Pm10.Should().BeApproximately(16.2, 0.0001);
                points[1].Pm25.Should().BeApproximately(9.1, 0.0001);
                points[1].UsAqi.Should().Be(45);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly air quality should return null when response is null.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldReturnNull_WhenResponseIsNull()
        {
            // Arrange - empty response
            var json = "{}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("0", "0", 24, logger, CancellationToken.None);

                // Assert
                points.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly air quality should return null when hourly data is absent.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldReturnNull_WhenHourlyDataAbsent()
        {
            // Arrange - response without hourly block
            var json = """
                {
                  "latitude": 12.34,
                  "longitude": 56.78
                }
                """;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("12.34", "56.78", 48, logger, CancellationToken.None);

                // Assert
                points.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly air quality should return null when time array is empty.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldReturnNull_WhenTimeArrayIsEmpty()
        {
            // Arrange - hourly block with empty arrays
            var airQualityObj = new
            {
                hourly = new
                {
                    time = Array.Empty<string>(),
                    pm10 = Array.Empty<double>(),
                    pm2_5 = Array.Empty<double>(),
                    carbon_monoxide = Array.Empty<double>(),
                    nitrogen_dioxide = Array.Empty<double>(),
                    sulphur_dioxide = Array.Empty<double>(),
                    ozone = Array.Empty<double>(),
                    us_aqi = Array.Empty<int>(),
                    european_aqi = Array.Empty<int>()
                }
            };

            var json = JsonSerializer.Serialize(airQualityObj);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("12.34", "56.78", 24, logger, CancellationToken.None);

                // Assert
                points.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly air quality should correctly mark current hour.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldCorrectlyMarkCurrentHour()
        {
            // Arrange - three hours: past, current, future
            var pastHour = DateTime.Now.AddHours(-1).ToString("yyyy-MM-ddTHH:00");
            var currentHour = DateTime.Now.ToString("yyyy-MM-ddTHH:00");
            var futureHour = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:00");

            var airQualityObj = new
            {
                hourly = new
                {
                    time = new[] { pastHour, currentHour, futureHour },
                    pm10 = new[] { 10.0, 11.0, 12.0 },
                    pm2_5 = new[] { 5.0, 6.0, 7.0 },
                    carbon_monoxide = new[] { 0.2, 0.3, 0.4 },
                    nitrogen_dioxide = new[] { 10.0, 11.0, 12.0 },
                    sulphur_dioxide = new[] { 1.0, 2.0, 3.0 },
                    ozone = new[] { 40.0, 41.0, 42.0 },
                    us_aqi = new[] { 30, 35, 40 },
                    european_aqi = new[] { 28, 32, 36 }
                }
            };

            var json = JsonSerializer.Serialize(airQualityObj);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("12.34", "56.78", 3, logger, CancellationToken.None);

                // Assert
                points.Should().NotBeNull();
                points.Should().HaveCount(3);

                // Past hour should not be marked as current
                points![0].Time.Should().Be(pastHour);
                points[0].IsCurrent.Should().BeFalse();

                // Current hour should be marked as current
                points[1].Time.Should().Be(currentHour);
                points[1].IsCurrent.Should().BeTrue();

                // Future hour should not be marked as current
                points[2].Time.Should().Be(futureHour);
                points[2].IsCurrent.Should().BeFalse();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly air quality should handle large forecast hours parameter.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldHandleLargeForecastHours()
        {
            // Arrange - generate 96 hours of data
            var timeValues = Enumerable.Range(0, 96)
                .Select(i => DateTime.Now.AddHours(i).ToString("yyyy-MM-ddTHH:00"))
                .ToArray();

            var airQualityObj = new
            {
                hourly = new
                {
                    time = timeValues,
                    pm10 = Enumerable.Range(0, 96).Select(i => 10.0 + i * 0.1).ToArray(),
                    pm2_5 = Enumerable.Range(0, 96).Select(i => 5.0 + i * 0.05).ToArray(),
                    carbon_monoxide = Enumerable.Range(0, 96).Select(i => 0.2 + i * 0.01).ToArray(),
                    nitrogen_dioxide = Enumerable.Range(0, 96).Select(i => 10.0 + i * 0.1).ToArray(),
                    sulphur_dioxide = Enumerable.Range(0, 96).Select(i => 1.0 + i * 0.02).ToArray(),
                    ozone = Enumerable.Range(0, 96).Select(i => 40.0 + i * 0.5).ToArray(),
                    us_aqi = Enumerable.Range(0, 96).Select(i => 30 + i).ToArray(),
                    european_aqi = Enumerable.Range(0, 96).Select(i => 28 + i).ToArray()
                }
            };

            var json = JsonSerializer.Serialize(airQualityObj);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("12.34", "56.78", 96, logger, CancellationToken.None);

                // Assert
                points.Should().NotBeNull();
                points.Should().HaveCount(96);
                points![0].Pm10.Should().BeApproximately(10.0, 0.0001);
                points[95].Pm10.Should().BeApproximately(19.5, 0.0001);
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        /// <summary>
        /// Builds the hourly air quality should handle HTTP errors gracefully.
        /// </summary>
        [Fact]
        public async Task BuildHourlyAirQuality_ShouldHandleHttpErrorsGracefully()
        {
            // Arrange - simulate HTTP error
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Server Error", Encoding.UTF8, "text/plain")
            };

            ReplaceSharedHttpClient(new DelegatingHandlerStub(response));
            try
            {
                var logger = NullLogger.Instance;

                // Act
                var points = await OpenMeteoHelper.BuildHourlyAirQualityAsync("12.34", "56.78", 24, logger, CancellationToken.None);

                // Assert - should handle error gracefully and return null
                points.Should().BeNull();
            }
            finally
            {
                RestoreOriginalHttpClient();
            }
        }

        #endregion
    }
    }