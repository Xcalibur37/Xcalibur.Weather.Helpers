# Xcalibur.Weather.Helpers

![Version](https://img.shields.io/badge/version-1.0.11-blue)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
[![NuGet](https://img.shields.io/nuget/v/Xcalibur.Weather.Helpers.svg)](https://www.nuget.org/packages/Xcalibur.Weather.Helpers/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE-2.0.txt)

A comprehensive .NET helper library providing utility functions for weather-related operations. Includes conversion helpers for temperature, wind speed, length, and pressure, along with specialized helpers for Open-Meteo, Geocodio, IpGeolocation.io, Atmospore, SunriseSunset.io, OpenStreetMap, and combined weather alert operations (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD).

**Created by**: Joshua Arzt | **Company**: Xcalibur Systems, LLC.

## Purpose

**Xcalibur.Weather.Helpers** is designed to:

- Provide high-level helper utilities that simplify working with weather data
- Enable easy conversion between common weather measurement units (temperature, wind speed, pressure, length)
- Offer convenient wrapper methods around Xcalibur.Weather.Services for common use cases
- Deliver intelligent alert consolidation to eliminate duplicate weather alerts
- Support geographic region detection and routing for multi-provider weather services
- Reduce boilerplate code when building weather applications
- Provide strongly-typed, easy-to-use APIs for weather data transformation

## 💡 Use Cases

This library is ideal for:

- **Weather Applications**: Mobile and desktop apps requiring weather data with automatic unit conversion
- **Dashboard & UI Development**: Applications needing consolidated, display-ready weather alerts without duplicates
- **Smart Home Systems**: IoT devices requiring weather-based automation with simplified data access
- **Agricultural Solutions**: Farm management systems with easy access to weather, pollen, and alert data
- **Travel & Navigation Apps**: Location-based weather with intelligent multi-provider alert aggregation
- **Health & Wellness Apps**: Allergy tracking with simplified pollen data access
- **Emergency Management**: Systems requiring consolidated multi-source weather alerts
- **Data Transformation**: Backend services needing unit conversion and data normalization
- **Prototyping & MVPs**: Rapid development with high-level helper methods
- **Web APIs**: REST services exposing weather data with built-in unit conversion

## 🎉 What's New in v1.0.11

**Consolidated Weather Alert Methods** - A complete suite of consolidated alert methods for cleaner, more efficient alert handling:

- ✅ **All 7 providers now have consolidated versions**: NWS, Meteoalarm, Environment Canada, BOM, GDACS, DWD, and EMSC
- ✅ **Automatic duplicate removal**: Overlapping alerts of the same event type are consolidated, keeping only the highest severity
- ✅ **Clean, simple API**: Returns `IReadOnlyList<WeatherAlertItem>` directly - no tuples, no null checks needed
- ✅ **Optimized for UI display**: Perfect for weather apps that need to show unique, relevant alerts without duplicates
- ✅ **Smart consolidation**: Handles escalating alerts (e.g., Winter Weather Advisory → Winter Storm Warning) intelligently

**Example:**
```csharp
// Old way - might show duplicate alerts for the same weather event
var alerts = await WeatherAlertHelper.BuildNwsAlertsAsync(lat, lon, logger, token);

// New way - automatically consolidated, unique alerts only
var consolidated = await WeatherAlertHelper.BuildNwsAlertsConsolidatedAsync(lat, lon, logger, token);
foreach (var alert in consolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}
```

**Available Consolidated Methods:**
- `BuildCombinedAlertsConsolidatedAsync` - All providers combined
- `BuildNwsAlertsConsolidatedAsync` - US weather alerts
- `BuildMeteoalarmAlertsConsolidatedAsync` - European alerts
- `BuildEnvironmentCanadaAlertsConsolidatedAsync` - Canadian alerts
- `BuildBomAlertsConsolidatedAsync` - Australian alerts
- `BuildGdacsAlertsConsolidatedAsync` - Global disaster alerts
- `BuildDwdAlertsConsolidatedAsync` - German weather warnings
- `BuildEmscAlertsConsolidatedAsync` - Earthquake/seismic alerts

## 📋 Table of Contents

- [Purpose](#purpose)
- [Use Cases](#-use-cases)
- [What's New](#-whats-new-in-v1011)
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
- [Changelog](#changelog)
- [License](#license)
- [Related Projects](#related-projects)
- [Contributing](#contributing)

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
<PackageReference Include="Xcalibur.Weather.Helpers" Version="1.0.11" />
```

## Requirements

- **.NET 10.0** or later
- **Xcalibur.Weather.Services 1.0.11** (included as dependency)
- **Microsoft.Extensions.Hosting 10.0.9** (included as dependency)

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
// Intelligently selects services based on geographic location
var alerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None,
    provinceCode: null,  // Optional: for Canada (e.g., "ON", "BC")
    stateCode: null      // Optional: for Australia (e.g., "NSW", "VIC")
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

// BUILD WITH AUTOMATIC CONSOLIDATION (Recommended for UI display)
// Automatically consolidates overlapping alerts (e.g., Winter Weather Advisory + Winter Storm Warning)
// Keeps only the highest severity alert from each overlapping group
var consolidatedAlerts = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
    latitude: "39.4300996",
    longitude: "-77.804161",
    logger: logger,
    token: CancellationToken.None
);

// Simple and clean - just iterate the results
foreach (var alert in consolidatedAlerts)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
    Console.WriteLine($"  Source: {alert.Source}");
    Console.WriteLine($"  Effective: {alert.Effective}");
    Console.WriteLine($"  Expires: {alert.Expires}");
}

// MANUAL CONSOLIDATION (if you need access to the full CombinedWeatherAlertInformation object)
var fullAlerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(
    "39.4300996", "-77.804161", logger, CancellationToken.None);

if (fullAlerts is not null)
{
    Console.WriteLine($"Total Alerts: {fullAlerts.TotalAlerts}");
    Console.WriteLine($"Data Sources: {string.Join(", ", fullAlerts.DataSources)}");

    // Manually consolidate if needed
    var consolidated = WeatherAlertHelper.ConsolidateAlerts(fullAlerts.Alerts, logger);
    foreach (var alert in consolidated)
    {
        Console.WriteLine($"[{alert.Severity}] {alert.Event}");
    }
}

// Build alerts from specific services
var meteoalarmAlerts = await WeatherAlertHelper.BuildMeteoalarmAlertsAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None
);

var nwsAlerts = await WeatherAlertHelper.BuildNwsAlertsAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger,
    token: CancellationToken.None
);

// NWS with automatic consolidation (for US locations)
var nwsConsolidated = await WeatherAlertHelper.BuildNwsAlertsConsolidatedAsync(
    latitude: "39.4300996",
    longitude: "-77.804161",
    logger: logger,
    token: CancellationToken.None
);

// Clean and simple - just use the results directly
foreach (var alert in nwsConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}

var gdacsAlerts = await WeatherAlertHelper.BuildGdacsAlertsAsync(
    latitude: "35.6762",
    longitude: "139.6503",
    logger: logger,
    token: CancellationToken.None
);

var canadaAlerts = await WeatherAlertHelper.BuildEnvironmentCanadaAlertsAsync(
    latitude: "43.65",
    longitude: "-79.38",
    provinceCode: "ON",
    logger: logger,
    token: CancellationToken.None
);

// Environment Canada with automatic consolidation (for Canadian locations)
var canadaConsolidated = await WeatherAlertHelper.BuildEnvironmentCanadaAlertsConsolidatedAsync(
    latitude: "43.65",
    longitude: "-79.38",
    provinceCode: "ON",
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in canadaConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}

var bomAlerts = await WeatherAlertHelper.BuildBomAlertsAsync(
    latitude: "-33.87",
    longitude: "151.21",
    stateCode: "NSW",
    logger: logger,
    token: CancellationToken.None
);

// BOM with automatic consolidation (for Australian locations)
var bomConsolidated = await WeatherAlertHelper.BuildBomAlertsConsolidatedAsync(
    latitude: "-33.87",
    longitude: "151.21",
    stateCode: "NSW",
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in bomConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}

var meteoalarmAlerts = await WeatherAlertHelper.BuildMeteoalarmAlertsAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None
);

// Meteoalarm with automatic consolidation (for European locations)
var meteoalarmConsolidated = await WeatherAlertHelper.BuildMeteoalarmAlertsConsolidatedAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in meteoalarmConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}

var gdacsAlerts = await WeatherAlertHelper.BuildGdacsAlertsAsync(
    latitude: "35.6762",
    longitude: "139.6503",
    logger: logger,
    token: CancellationToken.None
);

// GDACS with automatic consolidation (for global disaster alerts)
var gdacsConsolidated = await WeatherAlertHelper.BuildGdacsAlertsConsolidatedAsync(
    latitude: "35.6762",
    longitude: "139.6503",
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in gdacsConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}

var dwdAlerts = await WeatherAlertHelper.BuildDwdAlertsAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None
);

// DWD with automatic consolidation (for German locations)
var dwdConsolidated = await WeatherAlertHelper.BuildDwdAlertsConsolidatedAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in dwdConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}

var emscAlerts = await WeatherAlertHelper.BuildEmscAlertsAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    radiusKm: 500,
    logger: logger,
    token: CancellationToken.None
);

// EMSC with automatic consolidation (for earthquake/seismic alerts)
var emscConsolidated = await WeatherAlertHelper.BuildEmscAlertsConsolidatedAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    radiusKm: 500,
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in emscConsolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}
```

### Weather Region Helper

```csharp
using Xcalibur.Weather.Helpers.Services;

// Determine geographic region from coordinates
var region = WeatherRegionHelper.DetermineRegion(
    latitude: 52.52,
    longitude: 13.41
);

Console.WriteLine($"Region: {region}"); // Output: Europe

// Check if coordinates are in Germany
bool isGermany = WeatherRegionHelper.IsInGermany(
    latitude: 52.52,
    longitude: 13.41
);

// Determine Canadian province from coordinates
var province = WeatherRegionHelper.DetermineCanadianProvince(
    latitude: 43.65,
    longitude: -79.38
);

Console.WriteLine($"Province: {province}"); // Output: ON

// Determine Australian state from coordinates
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
| `BuildCombinedAlertsAsync(string, string, ILogger, CancellationToken, string?, string?)` | Aggregates weather alerts from multiple global sources (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD) into a unified `CombinedWeatherAlertInformation` model. Intelligently selects services based on geographic location. Optional `provinceCode` for Canada and `stateCode` for Australia. |
| `BuildCombinedAlertsConsolidatedAsync(...)` | **Recommended for UI display.** Returns only the consolidated alerts list, removing overlapping duplicates and keeping the highest severity alert from each group. Returns an empty list if no alerts exist. Clean, simple API. |
| `ConsolidateAlerts(IEnumerable<WeatherAlertItem>, ILogger?)` | Consolidates a collection of alerts by removing overlapping duplicates and keeping the highest severity alert from each group. Use this for manual consolidation when you need access to the full `CombinedWeatherAlertInformation` object. |
| `BuildMeteoalarmAlertsAsync(string, string, ILogger, CancellationToken)` | Retrieves alerts from Meteoalarm (Europe) |
| `BuildMeteoalarmAlertsConsolidatedAsync(string, string, ILogger, CancellationToken)` | **Recommended for Europe.** Retrieves alerts from Meteoalarm with automatic consolidation. Returns a list of unique, consolidated alerts. |
| `BuildNwsAlertsAsync(string, string, ILogger, CancellationToken)` | Retrieves alerts from the US National Weather Service |
| `BuildNwsAlertsConsolidatedAsync(string, string, ILogger, CancellationToken)` | **Recommended for US.** Retrieves alerts from NWS with automatic consolidation. Returns a list of unique, consolidated alerts. |
| `BuildGdacsAlertsAsync(string, string, ILogger, CancellationToken)` | Retrieves global disaster alerts from GDACS |
| `BuildGdacsAlertsConsolidatedAsync(string, string, ILogger, CancellationToken)` | **Recommended for global disasters.** Retrieves alerts from GDACS with automatic consolidation. Returns a list of unique, consolidated alerts. |
| `BuildEnvironmentCanadaAlertsAsync(string, string, string, ILogger, CancellationToken)` | Retrieves alerts from Environment Canada (requires province code) |
| `BuildEnvironmentCanadaAlertsConsolidatedAsync(string, string, string, ILogger, CancellationToken)` | **Recommended for Canada.** Retrieves alerts from Environment Canada with automatic consolidation. Returns a list of unique, consolidated alerts. |
| `BuildBomAlertsAsync(string, string, string, ILogger, CancellationToken)` | Retrieves alerts from the Australian Bureau of Meteorology (requires state code) |
| `BuildBomAlertsConsolidatedAsync(string, string, string, ILogger, CancellationToken)` | **Recommended for Australia.** Retrieves alerts from BOM with automatic consolidation. Returns a list of unique, consolidated alerts. |
| `BuildDwdAlertsAsync(string, string, ILogger, CancellationToken)` | Retrieves alerts from the German weather service (DWD) |
| `BuildDwdAlertsConsolidatedAsync(string, string, ILogger, CancellationToken)` | **Recommended for Germany.** Retrieves alerts from DWD with automatic consolidation. Returns a list of unique, consolidated alerts. |
| `BuildEmscAlertsAsync(string, string, int, ILogger, CancellationToken)` | Retrieves earthquake/seismic alerts from EMSC within specified radius (in km) |
| `BuildEmscAlertsConsolidatedAsync(string, string, int, ILogger, CancellationToken)` | **Recommended for earthquakes.** Retrieves alerts from EMSC with automatic consolidation. Returns a list of unique, consolidated alerts. |

### WeatherRegionHelper

| Method | Description |
|--------|-------------|
| `DetermineRegion(double, double)` | Determines the geographic region (US, Canada, Europe, Australia, Other) based on coordinates |
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
- [Xcalibur.Weather.Services](https://www.nuget.org/packages/Xcalibur.Weather.Services/) (v1.0.11) - Weather service providers and models
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) (v10.0.9) - Hosting abstractions

## Changelog

### Version 1.0.11 (Latest)
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.11
- 📚 **Documentation**: Enhanced README with Purpose and Use Cases sections
- 🏗️ **Improved**: Project structure and formatting consistency
- 🔧 **Maintenance**: Minor improvements and dependency updates

### Version 1.0.10
- ✨ **New**: Added consolidated alert methods for all 7 weather alert providers
  - `BuildCombinedAlertsConsolidatedAsync` - Multi-provider consolidated alerts
  - `BuildNwsAlertsConsolidatedAsync` - US National Weather Service
  - `BuildMeteoalarmAlertsConsolidatedAsync` - European weather alerts
  - `BuildEnvironmentCanadaAlertsConsolidatedAsync` - Canadian weather alerts
  - `BuildBomAlertsConsolidatedAsync` - Australian Bureau of Meteorology
  - `BuildGdacsAlertsConsolidatedAsync` - Global disaster alerts
  - `BuildDwdAlertsConsolidatedAsync` - German weather warnings
  - `BuildEmscAlertsConsolidatedAsync` - Earthquake/seismic alerts
- 🔄 **Improved**: Automatic consolidation of overlapping alerts by event type and severity
- 🎯 **Optimized**: Clean API design returning `IReadOnlyList<WeatherAlertItem>` for easier consumption
- 📚 **Documentation**: Comprehensive examples for all consolidated methods
- ✅ **Testing**: 23 new tests covering all consolidated alert scenarios

### Version 1.0.9
- 📝 Updated README with version information
- 🔧 Minor improvements and bug fixes

### Version 1.0.8
- 🌍 Comprehensive weather alert support (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD)
- 🛠️ Helper utilities for Open-Meteo, Geocodio, IpGeolocation, Atmospore, SunriseSunset, OpenStreetMap
- 🔄 Conversion helpers for temperature, wind speed, length, and pressure
- 📍 Geographic region detection and routing

## License

This project is licensed under the Apache License 2.0. See the [LICENSE-2.0.txt](LICENSE-2.0.txt) file for details.

Copyright © 2006 - 2026, Xcalibur Systems, LLC - All Rights Reserved

## Related Projects

- **[Xcalibur.Weather.Services](https://www.nuget.org/packages/Xcalibur.Weather.Services/)** (v1.0.11) - HTTP client services for weather APIs and models ([GitHub](https://github.com/Xcalibur37/Xcalibur.Weather.Services))
- **[Xcalibur.Weather.Models](https://www.nuget.org/packages/Xcalibur.Weather.Models/)** (v1.0.11) - Core weather data models and DTOs (included in Xcalibur.Weather.Services) ([GitHub](https://github.com/Xcalibur37/Xcalibur.Weather.Models))

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests to improve the library.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

*Part of the Xcalibur Weather ecosystem for comprehensive weather data integration.*

## Author

**Joshua Arzt**  
Xcalibur Systems, LLC
