using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xcalibur.Weather.Helpers.Services;
using Xcalibur.Weather.Models.Testing;
using Xcalibur.Weather.Services.WeatherProvider.GooglePollen;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests covering the Google Pollen helper and service behavior.
    /// Note: GooglePollenHelper creates an internal HttpClient per call, so tests
    /// exercise the service directly with a stubbed HttpMessageHandler to verify
    /// deserialization and the behaviors the helper depends on.
    /// </summary>
    public sealed class GooglePollenHelperTests
    {
        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldDeserializeResponse()
        {
            // Arrange — representative JSON matching the real API shape
            var json =
                """
                {
                  "regionCode": "US",
                  "dailyInfo": [
                    {
                      "date": { "year": 2026, "month": 5, "day": 1 },
                      "pollenTypeInfo": [
                        {
                          "code": "GRASS",
                          "displayName": "Grass",
                          "inSeason": true,
                          "indexInfo": {
                            "code": "UPI_3",
                            "displayName": "High",
                            "value": 3,
                            "category": "High",
                            "indexDescription": "People with moderate or severe allergy symptoms to grass pollen should reduce time outdoors."
                          },
                          "healthRecommendations": ["Keep windows closed.", "Shower after outdoor activities."]
                        }
                      ],
                      "plantInfo": [
                        {
                          "code": "BERMUDA_GRASS",
                          "displayName": "Bermuda grass",
                          "inSeason": true,
                          "indexInfo": {
                            "code": "UPI_3",
                            "displayName": "High",
                            "value": 3,
                            "category": "High",
                            "indexDescription": "High pollen concentration."
                          }
                        }
                      ]
                    }
                  ]
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.RegionCode.Should().Be("US");
            result.DailyInfo.Should().HaveCount(1);

            var day = result.DailyInfo![0];
            day.Date.Should().NotBeNull();
            day.Date!.Year.Should().Be(2026);
            day.Date.Month.Should().Be(5);
            day.Date.Day.Should().Be(1);

            day.PollenTypeInfo.Should().HaveCount(1);
            var pollenType = day.PollenTypeInfo![0];
            pollenType.Code.Should().Be("GRASS");
            pollenType.DisplayName.Should().Be("Grass");
            pollenType.InSeason.Should().BeTrue();
            pollenType.IndexInfo.Should().NotBeNull();
            pollenType.IndexInfo!.Value.Should().Be(3);
            pollenType.IndexInfo.Category.Should().Be("High");
            pollenType.HealthRecommendations.Should().HaveCount(2);

            day.PlantInfo.Should().HaveCount(1);
            var plant = day.PlantInfo![0];
            plant.Code.Should().Be("BERMUDA_GRASS");
            plant.DisplayName.Should().Be("Bermuda grass");
            plant.InSeason.Should().BeTrue();
            plant.IndexInfo.Should().NotBeNull();
            plant.IndexInfo!.Value.Should().Be(3);
        }

        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldReturnNull_WhenResponseIsNotSuccess()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var http = new HttpClient(new DelegatingHandlerStub(response));
            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("0", "0", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldReturnNull_WhenResponseIsInvalidJson()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response));
            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("0", "0", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GooglePollenService_TestApiKey_ShouldReturnTrue_WhenResponseIsSuccess()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "VALID_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public async Task GooglePollenService_TestApiKey_ShouldReturnFalse_WhenResponseIsUnauthorized()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "BAD_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task GooglePollenService_TestApiKey_ShouldReturnFalse_WhenResponseIsForbidden()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "BAD_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var isValid = await service.TestApiKey(CancellationToken.None);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task BuildPollenForecastAsync_ShouldReturnNull_WhenApiKeyIsEmpty()
        {
            // Act — empty key should short-circuit before any HTTP call
            var result = await GooglePollenHelper.BuildPollenForecastAsync(
                apiKey: "",
                latitude: "39.43",
                longitude: "-77.80",
                logger: NullLogger.Instance);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task BuildPollenForecastAsync_ShouldReturnNull_WhenApiKeyIsWhiteSpace()
        {
            // Act — whitespace-only key should also short-circuit
            var result = await GooglePollenHelper.BuildPollenForecastAsync(
                apiKey: "   ",
                latitude: "39.43",
                longitude: "-77.80",
                logger: NullLogger.Instance);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldDeserializePlantDescription()
        {
            // Arrange — plant info entry with a fully-populated plantDescription block
            var json =
                """
                {
                  "regionCode": "US",
                  "dailyInfo": [
                    {
                      "date": { "year": 2026, "month": 5, "day": 1 },
                      "pollenTypeInfo": [],
                      "plantInfo": [
                        {
                          "code": "BIRCH",
                          "displayName": "Birch",
                          "inSeason": true,
                          "indexInfo": {
                            "code": "UPI_1",
                            "displayName": "Low",
                            "value": 1,
                            "category": "Low",
                            "indexDescription": "Low pollen concentration.",
                            "color": { "red": 0.5, "green": 0.75, "blue": 0.25 }
                          },
                          "plantDescription": {
                            "type": "TREE",
                            "family": "Betulaceae",
                            "season": "Late winter, spring",
                            "crossReaction": "Alder, hornbeam, hazel",
                            "picture": "https://example.com/birch.jpg",
                            "pictureCloseup": "https://example.com/birch_closeup.jpg"
                          }
                        }
                      ]
                    }
                  ]
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", CancellationToken.None);

            // Assert — PlantDescriptionModel fields
            var plant = result!.DailyInfo![0].PlantInfo![0];
            plant.Code.Should().Be("BIRCH");
            plant.InSeason.Should().BeTrue();

            var desc = plant.PlantDescription;
            desc.Should().NotBeNull();
            desc!.Type.Should().Be("TREE");
            desc.Family.Should().Be("Betulaceae");
            desc.Season.Should().Be("Late winter, spring");
            desc.CrossReaction.Should().Be("Alder, hornbeam, hazel");
            desc.Picture.Should().Be("https://example.com/birch.jpg");
            desc.PictureCloseup.Should().Be("https://example.com/birch_closeup.jpg");

            // Assert — ColorModel on IndexInfo
            var color = plant.IndexInfo!.Color;
            color.Should().NotBeNull();
            color!.Red.Should().BeApproximately(0.5, 1e-6);
            color.Green.Should().BeApproximately(0.75, 1e-6);
            color.Blue.Should().BeApproximately(0.25, 1e-6);
        }

        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldDeserializeMultipleDays()
        {
            // Arrange — two daily entries to verify the full list is mapped
            var json =
                """
                {
                  "regionCode": "US",
                  "dailyInfo": [
                    {
                      "date": { "year": 2026, "month": 5, "day": 1 },
                      "pollenTypeInfo": [],
                      "plantInfo": []
                    },
                    {
                      "date": { "year": 2026, "month": 5, "day": 2 },
                      "pollenTypeInfo": [],
                      "plantInfo": []
                    }
                  ]
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.DailyInfo.Should().HaveCount(2);
            result.DailyInfo![0].Date!.Day.Should().Be(1);
            result.DailyInfo![1].Date!.Day.Should().Be(2);
        }

        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldDeserializeMultiplePollenTypes()
        {
            // Arrange — one day with GRASS, TREE, and WEED pollen types
            var json =
                """
                {
                  "regionCode": "US",
                  "dailyInfo": [
                    {
                      "date": { "year": 2026, "month": 5, "day": 1 },
                      "pollenTypeInfo": [
                        {
                          "code": "GRASS",
                          "displayName": "Grass",
                          "inSeason": true,
                          "indexInfo": { "value": 3, "category": "High" }
                        },
                        {
                          "code": "TREE",
                          "displayName": "Tree",
                          "inSeason": true,
                          "indexInfo": { "value": 2, "category": "Moderate" }
                        },
                        {
                          "code": "WEED",
                          "displayName": "Weed",
                          "inSeason": false,
                          "indexInfo": { "value": 0, "category": "None" }
                        }
                      ],
                      "plantInfo": []
                    }
                  ]
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", CancellationToken.None);

            // Assert
            var types = result!.DailyInfo![0].PollenTypeInfo!;
            types.Should().HaveCount(3);

            types[0].Code.Should().Be("GRASS");
            types[0].InSeason.Should().BeTrue();
            types[0].IndexInfo!.Value.Should().Be(3);

            types[1].Code.Should().Be("TREE");
            types[1].InSeason.Should().BeTrue();
            types[1].IndexInfo!.Value.Should().Be(2);

            types[2].Code.Should().Be("WEED");
            types[2].InSeason.Should().BeFalse();
            types[2].IndexInfo!.Value.Should().Be(0);
        }

        [Fact]
        public async Task GooglePollenService_GetPollenForecastAsync_ShouldHandleEmptyDailyInfo()
        {
            // Arrange — valid response body but with no daily entries
            var json =
                """
                {
                  "regionCode": "US",
                  "dailyInfo": []
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var http = new HttpClient(new DelegatingHandlerStub(response))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var service = new GooglePollenService(http, "DUMMY_KEY", NullLogger<GooglePollenService>.Instance);

            // Act
            var result = await service.GetPollenForecastAsync("39.43", "-77.80", CancellationToken.None);

            // Assert — response is not null, but the list is empty
            result.Should().NotBeNull();
            result!.DailyInfo.Should().NotBeNull();
            result.DailyInfo.Should().BeEmpty();
        }
    }
}
