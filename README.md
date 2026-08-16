# Xcalibur.Weather.Helpers

![Version](https://img.shields.io/badge/version-1.1.7-blue)
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

## 🎉 What's New in v1.1.7

**Meteoalarm Feed Reliability Update** - Replaced runtime slug generation with an explicit country-lookup table:

- ✅ **Meteoalarm Country Lookup**: `WeatherAlertService.GetMeteoalarmAlertsAsync` now resolves country names against a curated `MeteoalarmFeedSlugs` dictionary (40 entries, case-insensitive)
  - Unsupported country names short-circuit immediately with a warning log and `null` return — no speculative HTTP requests
  - Irregular slugs such as `czechia` (Czech Republic) and `republic-of-north-macedonia` (North Macedonia) are mapped correctly, which the old runtime slugifier could not guarantee
  - Leading/trailing whitespace is trimmed before the lookup
- ✅ **`BuildCombinedAlertsAsync` Enhancement**: Added optional `countryName` parameter
  - When coordinates resolve to the Europe region, callers can supply a specific country name (e.g., `"Austria"`) for a targeted feed, or omit it to default to the `"Europe"` aggregate feed
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.1.6

**Benefits**: Meteoalarm alert retrieval is now predictable and fail-fast for unsupported regions, and callers can target specific European country feeds instead of always using the aggregate feed.

---

### Previous Release - v1.1.5

**Native AOT Observation Serialization Update** - Hardened JSON metadata coverage for weather observation flows:

- ✅ **Native AOT Compatibility Improvements**: NWS and METAR observation deserialization now include explicit source-generated JSON metadata coverage
  - Ensures observation payloads can be deserialized correctly in Native AOT scenarios
  - Reduces runtime serializer metadata gaps for NWS and METAR-backed responses
  - Improves reliability for trimmed and ahead-of-time compiled applications
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.1.5
- 🔗 **Maintained**: Dependency on Microsoft.Extensions.Hosting v10.0.11
- 🧪 **Testing**: Expanded helper coverage for `WeatherObservationHelper` and `TimeZoneHelper`
- 📦 **Packaging**: README and package metadata aligned for v1.1.5

**Benefits**: Applications using weather observation flows now have stronger compatibility with Native AOT deployments while retaining resilient NWS and METAR deserialization behavior.

---

### Previous Release - v1.1.0

**Observation, Timezone, and API Modernization Update** - Expanded helper coverage and cleaner public APIs:

- ✅ **New `WeatherObservationHelper`**: Added simplified access to regional weather observation services
  - `GetObservationAsync` for nearest observation lookup by string or numeric coordinates
  - `GetMultipleObservationsAsync` for nearby observation lists
  - `DetermineRegion` helpers for observation-region routing
  - Regional convenience methods for NWS, ECCC, and METAR observation retrieval
- ✅ **New `TimeZoneHelper`**: Added reusable timezone conversion extensions
  - `ConvertFromTimezone` for local timezone conversions
  - `ConvertFromTimezoneUtc` for UTC-based timezone conversions
- ✅ **OpenMeteo Enhancements**: Expanded forecast capabilities and improved timezone-aware mapping
  - New `BuildShortTermForecastAsync` for 15-minute forecast retrieval
  - `BuildHourlyForecastAsync`, `BuildDailyForecastAsync`, and `BuildHourlyAirQualityAsync` support `forecastDays` and `pastDays`
  - Current-day and current-hour mapping now use timezone-aware conversions
- ✅ **OpenStreetMap Improvements**: Enhanced geocoding helper inputs
  - `BuildAddressLocationsAsync` now accepts `languageCode` and `country`
  - Better request filtering for localized and country-specific searches
- ✅ **Weather Alert API Cleanup**: Streamlined public helper surface
  - `BuildCombinedAlertsAsync`, `BuildCombinedAlertsConsolidatedAsync`, and `ConsolidateAlerts` remain the supported public APIs
  - Consolidation logic keeps the highest-severity overlapping alert and logs consolidation decisions
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.1.0
- 🔗 **Updated**: Dependency on Microsoft.Extensions.Hosting v10.0.11
- 🧪 **Testing**: Added coverage for `WeatherObservationHelper` and `TimeZoneHelper`, and synchronized helper tests with the latest signatures
- 📦 **Packaging**: README and package metadata aligned for v1.1.0

**Benefits**: Applications can now retrieve observation data through a simple helper API, correctly map weather data across timezones, use more focused OpenStreetMap searches, and rely on a cleaner, consolidated alert surface.

---

### Previous Release - v1.0.24

**Historic Data Retrieval for OpenMeteo Functions** - Comprehensive forecast and historical data support:

- ✅ **Historic Forecast Support**: All OpenMeteo forecast functions now support historic data retrieval
  - `BuildDailyForecastAsync` now accepts `forecastDays` and `pastDays` parameters
  - `BuildHourlyForecastAsync` now accepts `forecastDays` and `pastDays` parameters
  - Enables consistent API pattern across all OpenMeteo forecast methods
- ✅ **Flexible Date Range Queries**: Query both future forecasts and historical weather data in a single call
  - Supports time-series analysis across past and future dates
  - Enables comparative studies and trend analysis
  - Preserves current-hour/current-day marking in returned points
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.24
- 🔗 **Maintained**: Dependency on Xcalibur.Weather.Models v1.0.21
- 🧪 **Testing**: Updated all forecast tests for the new method signatures
- 📦 **Packaging**: Package references aligned for v1.0.24

**Benefits**: Applications can now seamlessly retrieve weather forecast data spanning both historical and future periods, enabling richer visualizations, comparisons, and analytics.

---

### Previous Release - v1.0.22

**Historic Hourly Air Quality Support** - Expanded OpenMeteo air quality retrieval options:

- ✅ **Historic Hourly Air Quality Retrieval**: `BuildHourlyAirQualityAsync` now supports both forecast and past-day ranges
  - Added `forecastDays` and `pastDays` parameters for more flexible hourly air quality queries
  - Supports retrieval of historic air quality data alongside forecasted data
  - Improves range-based air quality analysis scenarios
- ✅ **OpenMeteo Air Quality API Enhancements**: Hourly air quality requests now support broader date coverage
  - Better support for historical and comparative air quality experiences
  - Preserves current-hour mapping for returned hourly points
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.22
- 🔗 **Maintained**: Dependency on Xcalibur.Weather.Models v1.0.21
- 🧪 **Testing**: Updated affected hourly air quality tests for the new method signature
- 📦 **Packaging**: Package references aligned for v1.0.22

**Benefits**: Applications can now retrieve hourly air quality data across both forecast and recent historical periods, enabling richer charts, comparisons, and trend analysis.

---

### Previous Release - v1.0.21

**Air Quality Index (AQI) Enhancements** - Comprehensive US and EU metrics support:

- ✅ **US AQI Metrics**: Full support for United States Air Quality Index standards
  - Individual pollutant tracking: PM2.5, PM10, NO₂, O₃, SO₂, CO
  - US AQI value with severity classification and health recommendations
  - Detailed descriptions for air quality levels (Good, Moderate, Unhealthy, etc.)
- ✅ **EU AQI Metrics**: Complete European Air Quality Index implementation
  - European pollutant standards and measurements
  - EU AQI value with European classification system
  - Separate metrics aligned with European air quality directives
- ✅ **Hourly Air Quality Forecasts**: New `BuildHourlyAirQualityAsync` method
  - Retrieve hourly air quality forecasts with configurable forecast hours
  - Automatic current hour detection and marking
  - Comprehensive pollutant data for both US and EU standards
- 🔗 **Updated**: Dependencies updated to v1.0.21
  - Xcalibur.Weather.Services v1.0.21
  - Xcalibur.Weather.Models v1.0.21
- 🧪 **Testing**: Added 7 comprehensive tests for BuildHourlyAirQualityAsync
- 📦 **Packaging**: All dependencies synchronized for .NET 10.0

**Benefits**: Applications can now provide detailed air quality information for both US and European users with region-specific AQI values, pollutant breakdowns, and health recommendations.

---

### Previous Release - v1.0.20

**Air Quality Enhancements** - Improved air quality data support:

- ✅ **Enhanced AQI Data**: Improved Air Quality Index data models
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.20
- 🔧 **Improved**: Air quality point data structures

---

### Previous Release - v1.0.19

**Enhanced OpenMeteo Functionality** - Expanded weather data support:

- ✅ **Relative Humidity Support**: Added relative humidity data to daily forecast models
  - Daily forecasts now include humidity information for better weather planning
  - Enhanced daily weather models with additional moisture data
  - Improved data completeness for agricultural and outdoor applications
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.19
  - Latest OpenMeteo API improvements
  - Enhanced data models with additional parameters
- 🔧 **Improved**: Daily forecast models enhanced with more meteorological data
- 📦 **Packaging**: Updated dependencies for better compatibility

---

### Previous Release - v1.0.18

**Supplemental Weather Data** - Enhanced forecast capabilities with additional data:

- ✅ **Supplemental Hourly Forecasts**: Added support for supplemental hourly weather data
  - Automatically fetches and merges additional forecast parameters
  - Provides more comprehensive hourly forecasts with extended data points
  - Enhanced `BuildHourlyForecast` method with supplemental data integration
- ✅ **Supplemental Daily Forecasts**: Added support for supplemental daily weather data
  - Enriched daily forecasts with additional meteorological parameters
  - Improved data completeness for multi-day forecasts
  - Enhanced `BuildDailyForecast` method with supplemental data support
- 🔗 **Updated**: Dependencies updated to v1.0.18
  - Xcalibur.Weather.Services v1.0.18
  - Xcalibur.Weather.Models v1.0.18
- 🔧 **Improved**: OpenMeteoHelper refactored for better data handling and integration
- 📦 **Packaging**: All dependencies up to date for .NET 10.0

**Benefits**: Applications now receive more complete weather forecasts with additional parameters automatically merged from supplemental API calls.

---

### Previous Release - v1.0.17

**Enhanced Multi-Language Support** - Improved compatibility and performance:

- ✅ **Weather Code Value Retired**: Removed `WeatherCodeValue` string property for improved multi-language compatibility
  - The WMO weather code integer is still available in forecast data
  - Applications can now implement locale-specific descriptions based on their language requirements
  - Reduces data model size and improves performance
- 🐛 **Bug Fixes**: Several improvements and bug fixes for model handling
- 🔧 **Model Changes**: Optimized data models for better performance
- ⚡ **Performance**: MVVM performance improvements
- 🧪 **Testing**: Updated test suite with improved coverage

**Migration Note**: If your application used the `WeatherCodeValue` property, you'll need to implement your own locale-specific weather code descriptions based on the WMO code integer.

---

### Previous Release - v1.0.16

**Consolidated Weather Alert Methods** - A complete suite of consolidated alert methods for cleaner, more efficient alert handling:

- ✅ **All 7 providers now have consolidated versions**: NWS, Meteoalarm, Environment Canada, BOM, GDACS, DWD, and EMSC
- ✅ **Automatic duplicate removal**: Overlapping alerts of the same event type are consolidated, keeping only the highest severity
- ✅ **Clean, simple API**: Returns `IReadOnlyList<WeatherAlertItem>` directly - no tuples, no null checks needed
- ✅ **Optimized for UI display**: Perfect for weather apps that need to show unique, relevant alerts without duplicates
- ✅ **Smart consolidation**: Handles escalating alerts (e.g., Winter Weather Advisory → Winter Storm Warning) intelligently

**Example:**
```csharp
// Build a full combined alert payload
var alerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(lat, lon, logger, token);

// Or return only unique consolidated alerts for UI display
var consolidated = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(lat, lon, logger, token);
foreach (var alert in consolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}
```

**Available Consolidation APIs:**
- `BuildCombinedAlertsConsolidatedAsync` - All providers combined
- `ConsolidateAlerts` - Manual consolidation for existing alert collections

## 📋 Table of Contents

- [Purpose](#purpose)
- [Use Cases](#-use-cases)
- [What's New](#-whats-new-in-v116)
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
- **OpenMeteoHelper**: Build air quality points, current forecasts, short-term forecasts, hourly forecasts, daily forecasts, and yesterday's data
- **GeocodioHelper**: Test API keys, build address locations from geocoding queries
- **IpGeoHelper**: Build sun/moon points and test API connectivity for astronomical data
- **AtmosporeHelper**: Test API keys, retrieve pollen forecasts from the Atmospore API
- **SunriseSunsetHelper**: Fetch sunrise/sunset and astronomical data from SunriseSunset.io — no API key required
- **OpenStreetMapHelper**: Geocode addresses using the OpenStreetMap Nominatim API — no API key required
- **WeatherAlertHelper**: Build combined weather alert information from multiple global services (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD)
- **WeatherObservationHelper**: Retrieve nearest and nearby weather observations with regional service routing
- **WeatherRegionHelper**: Determine geographic regions, check if coordinates are in Germany, determine Canadian provinces and Australian states
- **TimeZoneHelper**: Convert `DateTime` values using named timezone identifiers

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
<PackageReference Include="Xcalibur.Weather.Helpers" Version="1.1.6" />
```

## Requirements

- **.NET 10.0** or later
- **Xcalibur.Weather.Services 1.1.6** (included as dependency)
- **Microsoft.Extensions.Hosting 10.0.11** (included as dependency)

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
    token: CancellationToken.None
);

// Build current weather forecast
var currentForecast = await OpenMeteoHelper.BuildCurrentForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger,
    token: CancellationToken.None
);

// Build short-term (15-minute) forecast
var shortTermForecast = await OpenMeteoHelper.BuildShortTermForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger,
    token: CancellationToken.None
);

// Build hourly forecast
var hourlyForecast = await OpenMeteoHelper.BuildHourlyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    forecastDays: 2,
    pastDays: 0,
    logger: logger,
    token: CancellationToken.None
);

// Build daily forecast
var dailyForecast = await OpenMeteoHelper.BuildDailyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    forecastDays: 7,
    pastDays: 0,
    logger: logger,
    token: CancellationToken.None
);

// Build hourly air quality forecast/history
var hourlyAirQuality = await OpenMeteoHelper.BuildHourlyAirQualityAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    forecastDays: 2,
    pastDays: 1,
    logger: logger,
    token: CancellationToken.None
);

// Build yesterday's hourly forecast
var yesterdayHourlyForecast = await OpenMeteoHelper.BuildYesterdayHourlyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    dateValue: "2026-01-14",
    logger: logger,
    token: CancellationToken.None
);

// Build yesterday's daily forecast
var yesterdayDailyForecast = await OpenMeteoHelper.BuildYesterdayDailyForecastAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    startDateValue: "2026-01-13",
    endDateValue: "2026-01-14",
    logger: logger,
    token: CancellationToken.None
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
    languageCode: "en",
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

// Region-aware combined alerts are now the supported public alert API.
// Use optional provinceCode/stateCode when you want to override automatic region detection.
var canadaAlerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(
    latitude: "43.65",
    longitude: "-79.38",
    logger: logger,
    token: CancellationToken.None,
    provinceCode: "ON"
);

var australiaAlerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(
    latitude: "-33.87",
    longitude: "151.21",
    logger: logger,
    token: CancellationToken.None,
    stateCode: "NSW"
);

// For UI display, prefer the consolidated list.
var consolidated = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
    latitude: "52.52",
    longitude: "13.41",
    logger: logger,
    token: CancellationToken.None
);

foreach (var alert in consolidated)
{
    Console.WriteLine($"[{alert.Severity}] {alert.Event}");
}
```

### Weather Observation Helper

```csharp
using Xcalibur.Weather.Helpers.Services;
using Microsoft.Extensions.Logging;

// Get the nearest observation for coordinates
var observation = await WeatherObservationHelper.GetObservationAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger,
    token: CancellationToken.None
);

// Get several nearby observations
var nearbyObservations = await WeatherObservationHelper.GetMultipleObservationsAsync(
    latitude: "40.7128",
    longitude: "-74.0060",
    maxResults: 5,
    logger: logger,
    token: CancellationToken.None
);

// Determine the observation routing region
var observationRegion = WeatherObservationHelper.DetermineRegion("40.7128", "-74.0060");
Console.WriteLine($"Observation Region: {observationRegion}");
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
| `BuildCurrentForecastAsync(string, string, ILogger, CancellationToken)` | Retrieves and builds current weather forecast point |
| `BuildShortTermForecastAsync(string, string, ILogger, CancellationToken)` | Retrieves and builds 15-minute forecast points |
| `BuildHourlyForecastAsync(string, string, int, int, ILogger, CancellationToken)` | Retrieves and builds hourly forecast points across forecast and past-day ranges |
| `BuildDailyForecastAsync(string, string, int, int, ILogger, CancellationToken)` | Retrieves and builds daily forecast points across forecast and past-day ranges |
| `BuildYesterdayHourlyForecastAsync(string, string, string, ILogger, CancellationToken)` | Retrieves and builds yesterday's hourly forecast |
| `BuildYesterdayDailyForecastAsync(string, string, string, string, ILogger, CancellationToken)` | Retrieves and builds yesterday's daily forecast |
| `BuildHourlyAirQualityAsync(string, string, int, int, ILogger, CancellationToken)` | Retrieves and builds hourly air-quality points across forecast and past-day ranges |

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
| `BuildAddressLocationsAsync(string, string?, string?, ILogger?)` | Geocodes an address query via OpenStreetMap Nominatim with optional language and country filtering — no API key required |

### WeatherAlertHelper

| Method | Description |
|--------|-------------|
| `BuildCombinedAlertsAsync(string, string, ILogger, CancellationToken, string?, string?)` | Aggregates weather alerts from multiple global sources (Meteoalarm, NWS, GDACS, Environment Canada, BOM, EMSC, DWD) into a unified `CombinedWeatherAlertInformation` model. Intelligently selects services based on geographic location. Optional `provinceCode` for Canada and `stateCode` for Australia. |
| `BuildCombinedAlertsConsolidatedAsync(...)` | **Recommended for UI display.** Returns only the consolidated alerts list, removing overlapping duplicates and keeping the highest severity alert from each group. Returns an empty list if no alerts exist. Clean, simple API. |
| `ConsolidateAlerts(IEnumerable<WeatherAlertItem>, ILogger?)` | Consolidates a collection of alerts by removing overlapping duplicates and keeping the highest severity alert from each group. Use this for manual consolidation when you need access to the full `CombinedWeatherAlertInformation` object. |

### WeatherObservationHelper

| Method | Description |
|--------|-------------|
| `GetObservationAsync(string, string, ILogger, CancellationToken)` | Retrieves the nearest observation using string coordinates |
| `GetObservationAsync(double, double, ILogger, CancellationToken)` | Retrieves the nearest observation using numeric coordinates |
| `GetMultipleObservationsAsync(string, string, int, ILogger, CancellationToken)` | Retrieves multiple nearby observations using string coordinates |
| `GetMultipleObservationsAsync(double, double, int, ILogger, CancellationToken)` | Retrieves multiple nearby observations using numeric coordinates |
| `DetermineRegion(string, string)` | Determines the observation region from string coordinates |
| `DetermineRegion(double, double)` | Determines the observation region from numeric coordinates |
| `GetNwsObservationAsync(double, double, ILogger, CancellationToken)` | Retrieves an observation from NWS directly |
| `GetEcccObservationAsync(double, double, ILogger, CancellationToken)` | Retrieves an observation from ECCC directly |
| `GetMetarObservationAsync(double, double, ILogger, CancellationToken)` | Retrieves an observation from METAR directly |

### TimeZoneHelper

| Method | Description |
|--------|-------------|
| `ConvertFromTimezone(DateTime?, string?)` | Converts a nullable `DateTime` using the provided timezone |
| `ConvertFromTimezone(DateTime, string?)` | Converts a `DateTime` using the provided timezone |
| `ConvertFromTimezoneUtc(DateTime?, string?)` | Converts a nullable UTC `DateTime` from UTC into the provided timezone |
| `ConvertFromTimezoneUtc(DateTime, string?)` | Converts a UTC `DateTime` from UTC into the provided timezone |

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
| `OpenMeteoHelper` | Air quality, current, short-term, hourly, daily, and yesterday forecasts — including absent/empty response blocks and timezone-aware current-point assessment | Full public API |
| `GeocodioHelper` | Address location mapping (single and multiple results), null/empty/invalid-JSON responses, API key validation | Full public API |
| `IpGeoHelper` | Sun/moon point mapping, null/whitespace key guards, deserialization, and HTTP error responses | Full public API |
| `SunriseSunsetHelper` | Sun/moon point mapping, successful deserialization, HTTP error and invalid-JSON responses | Full public API |
| `OpenStreetMapHelper` | Address location mapping, `town` fallback, language/country filtering, empty/null/invalid-JSON/HTTP error responses | Full public API |
| `AtmosporeHelper` | Pollen forecast deserialization, API key validation, null/whitespace guards, HTTP error and invalid-JSON responses | Full public API |
| `WeatherAlertHelper` | Combined alert aggregation, consolidation behavior, overlap resolution, `countryName` parameter routing, and cancellation behavior | Full public API |
| `WeatherAlertService` | Meteoalarm country-lookup (all 40 supported feeds, case-insensitive, whitespace trim), non-European short-circuit, HTTP error handling | Full `GetMeteoalarmAlertsAsync` API |
| `WeatherObservationHelper` | Invalid coordinate handling, nearest observation routing, nearby observation retrieval, and string/double overload region determination | Focused public API coverage |
| `TimeZoneHelper` | Null handling and timezone conversion behavior across all nullable and non-nullable overloads | Full public API |
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
var forecast = await OpenMeteoHelper.BuildCurrentForecastAsync(
    latitude, longitude, logger, CancellationToken.None
);
```

### HttpClient Usage
Service helpers manage `HttpClient` usage internally, so callers can use the helper APIs directly without constructing provider service instances.

## Dependencies

This library depends on:
- [Xcalibur.Weather.Services](https://www.nuget.org/packages/Xcalibur.Weather.Services/) (v1.1.6) - Weather service providers and models
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) (v10.0.11) - Hosting abstractions

## Changelog

### Version 1.1.6 (Latest)
- ✨ **Improved**: Meteoalarm feed resolution replaced runtime slug-builder with a curated `MeteoalarmFeedSlugs` lookup
  - 40 European countries and the `"Europe"` aggregate feed supported
  - Unsupported countries short-circuit immediately with a warning — no speculative HTTP requests
  - Irregular slugs (`czechia`, `republic-of-north-macedonia`) mapped correctly
- ✨ **Improved**: `BuildCombinedAlertsAsync` accepts optional `countryName` for targeted European feed selection
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.1.6
- 🔗 **Maintained**: Dependency on Microsoft.Extensions.Hosting v10.0.11
- 🧪 **Testing**: Added `WeatherAlertServiceTests` (53 tests) covering full country lookup table, case sensitivity, HTTP error paths, and helper `countryName` routing
- 📦 **Packaging**: Package metadata and README synchronized for v1.1.6

### Version 1.1.5
- ✨ **Improved**: Native AOT compatibility for observation deserialization
  - Added explicit source-generated JSON metadata coverage for NWS observation payloads
  - Added explicit source-generated JSON metadata coverage for METAR observation payloads
  - Hardens serializer behavior for trimmed and ahead-of-time compiled applications
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.1.5
- 🔗 **Maintained**: Dependency on Microsoft.Extensions.Hosting v10.0.11
- 🧪 **Testing**: Expanded `WeatherObservationHelper` and `TimeZoneHelper` coverage for additional overload and edge-case behavior
- 📦 **Packaging**: Package metadata and README synchronized for v1.1.5

### Version 1.1.0
- ✨ **New**: `WeatherObservationHelper` for nearest and nearby observation retrieval
  - Supports string and numeric coordinates
  - Adds region detection for observation routing
  - Includes direct NWS, ECCC, and METAR observation helpers
- ✨ **New**: `TimeZoneHelper` extension methods for timezone conversion
- ✨ **New**: `BuildShortTermForecastAsync` for 15-minute OpenMeteo forecasts
- 🔄 **Improved**: OpenMeteo helpers now use timezone-aware current-day/current-hour mapping
- 🔄 **Improved**: `OpenStreetMapHelper.BuildAddressLocationsAsync` now accepts optional `languageCode` and `country`
- 🧹 **Changed**: `WeatherAlertHelper` public API is centered on combined aggregation and consolidation methods
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.1.0
- 🔗 **Updated**: Dependency on Microsoft.Extensions.Hosting v10.0.11
- 🧪 **Testing**: Added tests for `WeatherObservationHelper` and `TimeZoneHelper`, and synchronized helper tests with current signatures
- 📦 **Packaging**: Package metadata and README synchronized for v1.1.0

### Version 1.0.24
- ✨ **New**: Historic data retrieval for all OpenMeteo forecast functions
  - `BuildDailyForecastAsync` now accepts `forecastDays` and `pastDays` parameters
  - `BuildHourlyForecastAsync` now accepts `forecastDays` and `pastDays` parameters
  - Consistent API pattern across all OpenMeteo forecast methods
  - Enables querying both future forecasts and historical weather data
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.24
- 🔗 **Maintained**: Dependency on Xcalibur.Weather.Models v1.0.21
- 🧪 **Testing**: Updated all forecast tests for the new method signatures
- 📦 **Packaging**: Package references synchronized for v1.0.24

### Version 1.0.22
- ✨ **New**: Historic hourly air quality retrieval support
  - `BuildHourlyAirQualityAsync` now accepts `forecastDays` and `pastDays`
  - Supports retrieving historical and forecast hourly air quality data in a single flow
  - Preserves current-hour detection for mapped `AirQualityPoint` results
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.22
- 🔗 **Maintained**: Dependency on Xcalibur.Weather.Models v1.0.21
- 🧪 **Testing**: Updated hourly air quality tests for the new method signature
- 📦 **Packaging**: Package references synchronized for v1.0.22

### Version 1.0.21
- ✨ **New**: Air Quality Index (AQI) enhancements to account for US and EU metrics
  - Full US AQI support with individual pollutant tracking and health recommendations
  - Complete EU AQI implementation with European air quality standards
  - New `BuildHourlyAirQualityAsync` method for hourly air quality forecasts
  - Automatic current hour detection in air quality data
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.21
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Models v1.0.21
- 🧪 **Testing**: Added comprehensive test coverage for hourly air quality functionality
- 📦 **Packaging**: All dependencies synchronized to v1.0.21

### Version 1.0.20
- ✨ **New**: Enhanced air quality data support
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.20
- 🔧 **Improved**: Air quality point data structures

### Version 1.0.19
- ✨ **New**: Relative humidity support for daily forecasts
  - Added relative humidity data to daily forecast models
  - Enhanced daily weather models with moisture information
  - Improved data completeness for agricultural and outdoor activity planning
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.19
- 🔧 **Improved**: Daily forecast models enhanced with additional meteorological parameters
- 📦 **Packaging**: Updated dependencies for better compatibility

### Version 1.0.18
- ✨ **New**: Supplemental weather data support for hourly and daily forecasts
  - Added `GetHourlyForecastSupplementalAsync` method for enhanced hourly data
  - Added `GetDailyForecastSupplementalAsync` method for enhanced daily data
  - Automatic merging of supplemental forecast parameters
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.18
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Models v1.0.18
- 🔧 **Improved**: OpenMeteoHelper refactored for better supplemental data integration
- 📦 **Maintenance**: All test packages updated to latest versions
- ⚡ **Performance**: Optimized data mapping and processing

### Version 1.0.17
- 🌍 **Breaking Change**: Weather code value retired for multi-language compatibility
  - Removed `WeatherCodeValue` string property from forecast models
  - WMO weather code integer remains available for custom locale implementations
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.17
- 🐛 **Fixed**: Several improvements and bug fixes for model handling
- ⚡ **Performance**: MVVM performance improvements and optimizations
- 🔧 **Improved**: Model changes for better data handling
- 🧪 **Testing**: Several test updates and improvements

### Version 1.0.16
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.17
- 🔗 **Updated**: Dependency on Microsoft.Extensions.Hosting v10.0.10
- 📚 **Documentation**: Enhanced README with Purpose and Use Cases sections
- 🏗️ **Improved**: Project structure and formatting consistency
- 🔧 **Maintenance**: Minor improvements and dependency updates

### Version 1.0.15
- 🔗 **Updated**: Dependency on Xcalibur.Weather.Services v1.0.16
- 🔗 **Updated**: Dependency on Microsoft.Extensions.Hosting v10.0.10
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

- **[Xcalibur.Weather.Services](https://www.nuget.org/packages/Xcalibur.Weather.Services/)** (v1.1.6) - HTTP client services for weather APIs and models ([GitHub](https://github.com/Xcalibur37/Xcalibur.Weather.Services))

---

*Part of the Xcalibur Weather ecosystem for comprehensive weather data integration.*

## Author

**Joshua Arzt**  
Xcalibur Systems, LLC
