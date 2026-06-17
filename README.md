# Xcalibur.Weather.Helpers

![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
[![NuGet](https://img.shields.io/nuget/v/Xcalibur.Weather.Helpers.svg)](https://www.nuget.org/packages/Xcalibur.Weather.Helpers/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE-2.0.txt)

A comprehensive .NET helper library providing utility functions for weather-related operations. Includes conversion helpers for temperature, wind speed, length, and pressure, along with specialized helpers for Open-Meteo, Geocodio, IpGeolocation.io, Atmospore, SunriseSunset.io, OpenStreetMap, and combined weather alert operations (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD).

**Created by**: Joshua Arzt | **Company**: Xcalibur Systems, LLC.

## 📋 Table of Contents

- [Features](#features)
  - [Conversion Utilities](#conversion-utilities)
  - [Weather Service Helpers](#weather-service-helpers)
- [Installation](#installation)
- [Requirements](#requirements)
- [Usage](#usage)
  - [Temperature Conversion](#temperature-conversion)
  - [Wind Speed Conversion](#wind-speed-conversion)
  - [Length Conversion](#length-conversion)
  - [Pressure Conversion](#pressure-conversion)
  - [OpenMeteo Helper](#openmeteo-helper)
  - [Geocodio Helper](#geocodio-helper)
  - [IpGeolocation Helper](#ipgeolocation-helper)
  - [Atmospore Helper](#atmospore-helper)
  - [SunriseSunset Helper](#sunrisesunset-helper)
  - [OpenStreetMap Helper](#openstreetmap-helper)
  - [Weather Alert Helper](#weather-alert-helper)
  - [Weather Region Helper](#weather-region-helper)
- [API Overview](#api-overview)
- [Best Practices](#best-practices)
- [Testing](#testing)
- [Dependencies](#dependencies)
- [License](#license)
- [Related Projects](#related-projects)

## Features

### Conversion Utilities
- **Temperature Conversion**: Celsius ↔ Fahrenheit conversions with formatting options
- **Wind Speed Conversion**: Convert between km/h, mph, ft/s, m/s, and knots
- **Length Conversion**: Convert between millimeters and inches
- **Pressure Conversion**: Convert between hPa, inHg, and mmHg
- **Smart Formatting**: Format values with or without unit symbols

### Weather Service Helpers
- **OpenMeteoHelper**: Build air quality points, current forecasts, hourly forecasts, daily forecasts, and yesterday's data
- **GeocodioHelper**: Test API keys, build address locations from geocoding queries
- **IpGeoHelper**: Build sun/moon points and test API connectivity for astronomical data
- **AtmosporeHelper**: Test API keys, retrieve pollen forecasts from the Atmospore API
- **SunriseSunsetHelper**: Fetch sunrise/sunset and astronomical data from SunriseSunset.io — no API key required
- **OpenStreetMapHelper**: Geocode addresses using the OpenStreetMap Nominatim API — no API key required
- **WeatherAlertHelper**: Build combined weather alert information from multiple global services (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD)
- **WeatherRegionHelper**: Determine geographic regions, check if coordinates are in Germany, determine Canadian provinces and Australian states

## Installation

### NuGet Package Manager
```bash
Install-Package Xcalibur.Weather.Helpers
```

### .NET CLI
```bash
dotnet add package Xcalibur.Weather.Helpers
```

### Package Reference
```xml
<PackageReference Include="Xcalibur.Weather.Helpers" Version="1.0.3" />
```

## Requirements

- **.NET 10** or later
- **Xcalibur.Weather.Models** (included as dependency)
- **Microsoft.Extensions.Hosting** (included as dependency)

## Usage

### Temperature Conversion

```csharp
using Xcalibur.Weather.Helpers;
using Xcalibur.Weather.Models;

// Convert Celsius to Fahrenheit
double celsius = 25.0;
double fahrenheit = celsius.CelsiusToFahrenheit(); // 77.0

// Convert Fahrenheit to Celsius
double temp = 77.0;
double celsiusValue = temp.FahrenheitToCelsius(); // 25.0

// Format temperature with unit
string formatted = celsius.FormatTemperature(TemperatureUnits.Fahrenheit, includeUnit: true);
// Output: "77°F"
```

### Wind Speed Conversion

```csharp
using Xcalibur.Weather.Helpers;
using Xcalibur.Weather.Models;

// Convert wind speed from km/h to various units
double windSpeed = 100.0; // km/h

double mph = windSpeed.ConvertWindSpeed(WindSpeedUnits.Mph);        // 62.14
double mps = windSpeed.ConvertWindSpeed(WindSpeedUnits.MSec);       // 27.78
double knots = windSpeed.ConvertWindSpeed(WindSpeedUnits.Knots);    // 53.99
double fps = windSpeed.ConvertWindSpeed(WindSpeedUnits.FtSec);      // 91.13
```

### Length Conversion

```csharp
using Xcalibur.Weather.Helpers;
using Xcalibur.Weather.Models;

// Format precipitation in different units
double? precipitation = 25.4; // millimeters

string metric = precipitation.FormatLength(DistanceUnits.Metric, includeUnit: true);
// Output: "25.40 mm"

string imperial = precipitation.FormatLength(DistanceUnits.Imperial, includeUnit: true);
// Output: "1.00 in"
```

### Pressure Conversion

```csharp
using Xcalibur.Weather.Helpers;
using Xcalibur.Weather.Models;

// Format atmospheric pressure in different units
double? pressure = 1013.25; // hectopascals

string hPa = pressure.FormatPressure(BarometerUnits.HPa, includeUnit: true);
// Output: "1013.25 hPa"

string inHg = pressure.FormatPressure(BarometerUnits.InHg, includeUnit: true);
// Output: "29.92 inHg"

string mmHg = pressure.FormatPressure(BarometerUnits.MmHg, includeUnit: true);
// Output: "760.00 mmHg"
```

### OpenMeteo Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Build air quality data point
var airQuality = await OpenMeteoHelper.BuildAirQualityPointAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger,
    cancellationToken: CancellationToken.None
);

// Build current weather forecast
var currentForecast = await OpenMeteoHelper.BuildCurrentForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    canAssessDayNight: true,
    sunrise: new TimeOnly(6, 30),
    sunset: new TimeOnly(18, 30),
    logger: logger,
    cancellationToken: CancellationToken.None
);

// Build hourly forecast
var hourlyForecast = await OpenMeteoHelper.BuildHourlyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    canAssessDayNight: true,
    sunrise: new TimeOnly(6, 30),
    sunset: new TimeOnly(18, 30),
    logger: logger,
    cancellationToken: CancellationToken.None
);

// Build daily forecast
var dailyForecast = await OpenMeteoHelper.BuildDailyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    forecastDays: 7,
    logger: logger,
    cancellationToken: CancellationToken.None
);

// Build yesterday's hourly forecast
var yesterdayHourlyForecast = await OpenMeteoHelper.BuildYesterdayHourlyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    canAssessDayNight: true,
    sunrise: new TimeOnly(6, 30),
    sunset: new TimeOnly(18, 30),
    logger: logger,
    cancellationToken: CancellationToken.None
);

// Build yesterday's daily forecast
var yesterdayDailyForecast = await OpenMeteoHelper.BuildYesterdayDailyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger,
    cancellationToken: CancellationToken.None
);
```

### Geocodio Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Test Geocodio API key
bool isValid = await GeocodioHelper.TestApiKeyAsync(
    apiKey: "your-api-key",
    logger: logger
);

// Build address locations from query
var locations = await GeocodioHelper.BuildAddressLocationsAsync(
    apiKey: "your-api-key",
    query: "1600 Pennsylvania Avenue NW, Washington, DC",
    country: "US",
    logger: logger
);
```

### IpGeolocation Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Test IpGeolocation API key
bool isValid = await IpGeoHelper.TestApiKeyAsync(
    apiKey: "your-api-key",
    logger: logger
);

// Build sun/moon astronomical data
var sunMoonData = await IpGeoHelper.BuildSunMoonPointAsync(
    ipGeoApiKey: "your-api-key",
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger
);
```

### Atmospore Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Test Atmospore API key
bool isValid = await AtmosporeHelper.TestApiKeyAsync(
    apiKey: "your-api-key",
    logger: logger
);

// Build pollen forecast
var pollenForecast = await AtmosporeHelper.BuildPollenForecastAsync(
    apiKey: "your-api-key",
    latitude: "39.43",
    longitude: "-77.80",
    date: "2024-05-27", // Optional, defaults to today
    forecastDays: 1,
    logger: logger
);

if (pollenForecast is not null)
{
    Console.WriteLine($"Date: {pollenForecast.ForecastDate}");
    foreach (var entry in pollenForecast.Entries)
    {
        Console.WriteLine($"{entry.DisplayName}: {entry.RiskLevel}");
    }
}
```

### SunriseSunset Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Build sun/moon data — no API key required
var sunMoonData = await SunriseSunsetHelper.BuildSunMoonPointAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger
);
```

### OpenStreetMap Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Geocode an address — no API key required
var locations = await OpenStreetMapHelper.BuildAddressLocationsAsync(
    query: "1600 Pennsylvania Avenue NW, Washington, DC",
    country: "US",
    logger: logger
);
```

### Weather Alert Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Build combined weather alerts from multiple sources
// (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD)
var alerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(
    latitude: 52.52,
    longitude: 13.41,
    countryCode: "DE",
    logger: logger
);

if (alerts is not null && alerts.Alerts.Any())
{
    Console.WriteLine($"Active Alerts: {alerts.TotalAlerts}");
    foreach (var alert in alerts.Alerts)
    {
        Console.WriteLine($"[{alert.Severity}] {alert.Event}");
        Console.WriteLine($"  Source: {alert.Source}");
        Console.WriteLine($"  Effective: {alert.Effective}");
        Console.WriteLine($"  Expires: {alert.Expires}");
    }
}
```

### Weather Region Helper

```csharp
using Xcalibur.Weather.Helpers.Services;

// Determine geographic region
var region = WeatherRegionHelper.DetermineRegion(
    latitude: 52.52,
    longitude: 13.41,
    countryCode: "DE"
);

Console.WriteLine($"Region: {region}"); // Output: Europe

// Check if coordinates are in Germany
bool isGermany = WeatherRegionHelper.IsInGermany(
    latitude: 52.52,
    longitude: 13.41
);

// Determine Canadian province
var province = WeatherRegionHelper.DetermineCanadianProvince(
    latitude: 43.65,
    longitude: -79.38
);

Console.WriteLine($"Province: {province}"); // Output: ON

// Determine Australian state
var state = WeatherRegionHelper.DetermineAustralianState(
    latitude: -33.87,
    longitude: 151.21
);

Console.WriteLine($"State: {state}"); // Output: NSW
```

## API Overview

### ConversionHelper

| Method | Description |
|--------|-------------|
| `CelsiusToFahrenheit(double)` | Converts temperature from Celsius to Fahrenheit |
| `CelsiusToFahrenheit(double?, double)` | Converts nullable Celsius to Fahrenheit; returns `defaultValue` when null |
| `FahrenheitToCelsius(double)` | Converts temperature from Fahrenheit to Celsius |
| `FahrenheitToCelsius(double?, double)` | Converts nullable Fahrenheit to Celsius; returns `defaultValue` when null |
| `ConvertWindSpeed(double, WindSpeedUnits?)` | Converts wind speed from km/h to specified unit |
| `ConvertWindSpeed(double?, WindSpeedUnits?)` | Converts nullable wind speed; returns `0` when null |
| `FormatTemperature(double, TemperatureUnits?, bool)` | Formats temperature with optional unit symbol |
| `FormatTemperature(double?, TemperatureUnits?, bool)` | Formats nullable temperature; returns empty string when null |
| `FormatLength(double?, DistanceUnits, bool)` | Formats length/precipitation with optional unit symbol |
| `FormatPressure(double?, BarometerUnits, bool)` | Formats pressure with optional unit symbol |

### OpenMeteoHelper

| Method | Description |
|--------|-------------|
| `BuildAirQualityPointAsync(string, string, ILogger, CancellationToken)` | Retrieves and builds air quality data for coordinates |
| `BuildCurrentForecastAsync(...)` | Retrieves and builds current weather forecast point |
| `BuildHourlyForecastAsync(...)` | Retrieves and builds hourly forecast points |
| `BuildDailyForecastAsync(string, string, int, ILogger, CancellationToken)` | Retrieves and builds daily forecast points |
| `BuildYesterdayHourlyForecastAsync(...)` | Retrieves and builds yesterday's hourly forecast |
| `BuildYesterdayDailyForecastAsync(string, string, ILogger, CancellationToken)` | Retrieves and builds yesterday's daily forecast |

### GeocodioHelper

| Method | Description |
|--------|-------------|
| `TestApiKeyAsync(string, ILogger)` | Tests the validity of a Geocodio API key |
| `BuildAddressLocationsAsync(...)` | Geocodes an address query and builds location models |

### IpGeoHelper

| Method | Description |
|--------|-------------|
| `TestApiKeyAsync(string, ILogger)` | Tests the validity of an IpGeolocation API key |
| `BuildSunMoonPointAsync(string, string, string, ILogger)` | Retrieves and builds sun/moon astronomical data |

### AtmosporeHelper

| Method | Description |
|--------|-------------|
| `TestApiKeyAsync(string, ILogger)` | Tests the validity of an Atmospore API key |
| `BuildPollenForecastAsync(string, string, string, string?, int, ILogger?)` | Retrieves and maps Atmospore pollen forecast data to a `PollenInformation` model |

### SunriseSunsetHelper

| Method | Description |
|--------|-------------|
| `BuildSunMoonPointAsync(string, string, ILogger?)` | Fetches sunrise/sunset data from SunriseSunset.io and maps it to a `SunMoonPoint` — no API key required |

### OpenStreetMapHelper

| Method | Description |
|--------|-------------|
| `BuildAddressLocationsAsync(string, string, ILogger?)` | Geocodes an address query via OpenStreetMap Nominatim and returns location models — no API key required |

### WeatherAlertHelper

| Method | Description |
|--------|-------------|
| `BuildCombinedAlertsAsync(double, double, string?, ILogger?, CancellationToken)` | Aggregates weather alerts from multiple global sources (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD) into a unified `WeatherAlertInformation` model |
| `BuildMeteoalarmAlertsAsync(...)` | Retrieves alerts from Meteoalarm (Europe) |
| `BuildNwsAlertsAsync(...)` | Retrieves alerts from the US National Weather Service |
| `BuildGdacsAlertsAsync(...)` | Retrieves global disaster alerts from GDACS |
| `BuildEnvironmentCanadaAlertsAsync(...)` | Retrieves alerts from Environment Canada |
| `BuildBomAlertsAsync(...)` | Retrieves alerts from the Australian Bureau of Meteorology |
| `BuildEmscAlertsAsync(...)` | Retrieves earthquake/seismic alerts from EMSC |
| `BuildDwdAlertsAsync(...)` | Retrieves alerts from the German weather service (DWD) |

### WeatherRegionHelper

| Method | Description |
|--------|-------------|
| `DetermineRegion(double, double, string?)` | Determines the geographic region (US, Canada, Europe, Australia, Other) based on coordinates and optional country code |
| `IsInGermany(double, double)` | Checks if coordinates fall within German geographic bounds |
| `DetermineCanadianProvince(double, double)` | Returns the two-letter Canadian province code for the given coordinates |
| `DetermineAustralianState(double, double)` | Returns the Australian state code for the given coordinates |

## Testing

The library ships with a comprehensive xUnit test suite covering all helpers and conversion utilities.

### Test Coverage

| Area | Tests | Coverage |
|------|-------|----------|
| `ConversionHelper` | Temperature, wind speed, length, and pressure conversions and formatting — including nullable overloads, null-unit guards, near-zero normalisation, and invalid-unit exceptions | Full public API |
| `OpenMeteoHelper` | Air quality, current, hourly, daily, and yesterday forecasts — including absent/empty response blocks and day/night assessment | Full public API |
| `GeocodioHelper` | Address location mapping (single and multiple results), null/empty/invalid-JSON responses, API key validation | Full public API |
| `IpGeoHelper` | Sun/moon point mapping, null/whitespace key guards, deserialization, and HTTP error responses | Full public API |
| `SunriseSunsetHelper` | Sun/moon point mapping, successful deserialization, HTTP error and invalid-JSON responses | Full public API |
| `OpenStreetMapHelper` | Address location mapping, `town` fallback, empty/null/invalid-JSON/HTTP error responses | Full public API |
| `AtmosporeHelper` | Pollen forecast deserialization, API key validation, null/whitespace guards, HTTP error and invalid-JSON responses | Full public API |
| `WeatherAlertHelper` | Combined alert aggregation, individual source helpers (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD), cancellation behavior | Full public API |
| `WeatherRegionHelper` | Region determination (US, Canada, Europe, Australia), Germany bounds check, Canadian province detection, Australian state detection | Full public API |

### Running the Tests

```bash
dotnet test
```

Or via the .NET CLI targeting the test project directly:

```bash
dotnet test Xcalibur.Weather.Helpers.Tests/Xcalibur.Weather.Helpers.Tests.csproj
```

## Best Practices

### Null Handling
All conversion methods include overloads that handle nullable values:

```csharp
double? temperature = null;
double result = temperature.CelsiusToFahrenheit(defaultValue: 0); // Returns 0
```

### Logging
All service helpers accept an `ILogger` parameter for diagnostics and troubleshooting:

```csharp
using Microsoft.Extensions.Logging;

ILogger logger = loggerFactory.CreateLogger<YourClass>();
var forecast = await OpenMeteoHelper.BuildCurrentForecast(
    latitude, longitude, true, sunrise, sunset, logger
);
```

### HttpClient Usage
Service helpers manage `HttpClient` usage internally, so callers can use the helper APIs directly without constructing provider service instances.

## Dependencies

This library depends on:
- [Xcalibur.Weather.Models](https://www.nuget.org/packages/Xcalibur.Weather.Models/) - Weather data models
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) - Hosting abstractions

## License

This project is licensed under the Apache License 2.0. See the [LICENSE-2.0.txt](LICENSE-2.0.txt) file for details.

Copyright © 2006 - 2026, Xcalibur Systems, LLC - All Rights Reserved

## Related Projects

- **[Xcalibur.Weather.Models](https://www.nuget.org/packages/Xcalibur.Weather.Models/)** - Core weather data models and DTOs ([GitHub](https://github.com/Xcalibur37/Xcalibur.Weather.Models))
- **[Xcalibur.Weather.Services](https://www.nuget.org/packages/Xcalibur.Weather.Services/)** - HTTP client services for weather APIs ([GitHub](https://github.com/Xcalibur37/Xcalibur.Weather.Services))

---

*Part of the Xcalibur Weather ecosystem for comprehensive weather data integration.*

## Author

**Joshua Arzt**  
Xcalibur Systems, LLC
