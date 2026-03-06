# Xcalibur.Weather.Helpers

![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
[![NuGet](https://img.shields.io/nuget/v/Xcalibur.Weather.Helpers.svg)](https://www.nuget.org/packages/Xcalibur.Weather.Helpers/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE-2.0.txt)

A comprehensive .NET helper library providing utility functions for weather-related operations. Includes conversion helpers for temperature, wind speed, length, and pressure, along with specialized helpers for Open-Meteo, Geocodio, and IpGeolocation.io weather data processing and transformation.

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
<PackageReference Include="Xcalibur.Weather.Helpers" Version="1.0.0" />
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
var airQuality = await OpenMeteoHelper.BuildAirQualityPoint(
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger
);

// Build current weather forecast
var currentForecast = await OpenMeteoHelper.BuildCurrentForecast(
    latitude: "40.7128",
    longitude: "-74.0060",
    canAssessDayNight: true,
    sunrise: new TimeOnly(6, 30),
    sunset: new TimeOnly(18, 30),
    logger: logger
);

// Build hourly forecast
var hourlyForecast = await OpenMeteoHelper.BuildHourlyForecast(
    latitude: "40.7128",
    longitude: "-74.0060",
    canAssessDayNight: true,
    sunrise: new TimeOnly(6, 30),
    sunset: new TimeOnly(18, 30),
    logger: logger
);

// Build daily forecast
var dailyForecast = await OpenMeteoHelper.BuildDailyForecast(
    latitude: "40.7128",
    longitude: "-74.0060",
    forecastDays: 7,
    logger: logger
);

// Build yesterday's forecast
var yesterdayForecast = await OpenMeteoHelper.BuildYesterdaysForecast(
    latitude: "40.7128",
    longitude: "-74.0060",
    canAssessDayNight: true,
    sunrise: new TimeOnly(6, 30),
    sunset: new TimeOnly(18, 30),
    logger: logger
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
var sunMoonData = await IpGeoHelper.BuildSunMoonPoint(
    ipGeoApiKey: "your-api-key",
    latitude: "40.7128",
    longitude: "-74.0060",
    logger: logger
);
```

## API Overview

### ConversionHelper

| Method | Description |
|--------|-------------|
| `CelsiusToFahrenheit(double)` | Converts temperature from Celsius to Fahrenheit |
| `FahrenheitToCelsius(double)` | Converts temperature from Fahrenheit to Celsius |
| `ConvertWindSpeed(double, WindSpeedUnits)` | Converts wind speed from km/h to specified unit |
| `FormatTemperature(double, TemperatureUnits, bool)` | Formats temperature with optional unit symbol |
| `FormatLength(double?, DistanceUnits, bool)` | Formats length/precipitation with optional unit symbol |
| `FormatPressure(double?, BarometerUnits, bool)` | Formats pressure with optional unit symbol |

### OpenMeteoHelper

| Method | Description |
|--------|-------------|
| `BuildAirQualityPoint(string, string, ILogger)` | Retrieves and builds air quality data for coordinates |
| `BuildCurrentForecast(...)` | Retrieves and builds current weather forecast point |
| `BuildHourlyForecast(...)` | Retrieves and builds hourly forecast points |
| `BuildDailyForecast(string, string, int, ILogger)` | Retrieves and builds daily forecast points |
| `BuildYesterdaysForecast(...)` | Retrieves and builds yesterday's hourly forecast |

### GeocodioHelper

| Method | Description |
|--------|-------------|
| `TestApiKeyAsync(string, ILogger)` | Tests the validity of a Geocodio API key |
| `BuildAddressLocationsAsync(...)` | Geocodes an address query and builds location models |

### IpGeoHelper

| Method | Description |
|--------|-------------|
| `TestApiKeyAsync(string, ILogger)` | Tests the validity of an IpGeolocation API key |
| `BuildSunMoonPoint(string, string, string, ILogger)` | Retrieves and builds sun/moon astronomical data |

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
Service helpers internally use a shared `HttpClient` instance for optimal resource usage and connection pooling.

## Dependencies

This library depends on:
- [Xcalibur.Weather.Models](https://www.nuget.org/packages/Xcalibur.Weather.Models/) - Weather data models
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) - Hosting abstractions

## Related Packages

- **Xcalibur.Weather.Models** - Core weather data models and enumerations
- **Xcalibur.Weather.Services** - HTTP client services for weather APIs

## License

Copyright © 2006 - 2026, Xcalibur Systems, LLC - All Rights Reserved

Licensed under the Apache License, Version 2.0. See [LICENSE-2.0.txt](LICENSE-2.0.txt) for details.

## Author

**Joshua Arzt**  
Xcalibur Systems, LLC

---

*Part of the Xcalibur Weather ecosystem for comprehensive weather data integration.*
