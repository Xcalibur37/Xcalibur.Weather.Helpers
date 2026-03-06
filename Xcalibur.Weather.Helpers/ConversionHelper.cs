using Xcalibur.Weather.Models;

namespace Xcalibur.Weather.Helpers;

/// <summary>
/// Conversion helper for temperature, wind speed, length, and pressure.
/// </summary>
public static class ConversionHelper
{
    #region Temperature Constants
    
    private const double CelsiusToFahrenheitRatio = 1.8;
    private const double FahrenheitOffset = 32.0;
    private const double FahrenheitToCelsiusRatio = 5.0 / 9.0;
    public const string DegreeSymbol = "\u00B0";
    
    #endregion

    #region Wind Speed Constants
    
    private const double KmhToMph = 0.621371;
    private const double KmhToFeetPerSecond = 0.911344;
    private const double KmhToMetersPerSecond = 0.277778;
    private const double KmhToKnots = 0.539957;
    
    #endregion

    #region Length Constants
    
    private const double MillimetersToInches = 0.0393701;
    
    #endregion

    #region Pressure Constants
    
    private const double HectopascalsToInchesOfMercury = 0.02952998057228;
    private const double HectopascalsToMillimetersOfMercury = 0.7500637554192;
    
    #endregion

    #region Temperature Conversion

    /// <summary>
    /// Converts Fahrenheit to Celsius.
    /// </summary>
    /// <param name="temp">The temperature in Fahrenheit.</param>
    /// <returns>Temperature in Celsius, rounded to 1 decimal place.</returns>
    public static double FahrenheitToCelsius(this double temp)
        => Math.Round((temp - FahrenheitOffset) * FahrenheitToCelsiusRatio, 1);

    /// <summary>
    /// Converts Fahrenheit to Celsius with a default value for null.
    /// </summary>
    /// <param name="temp">The temperature in Fahrenheit.</param>
    /// <param name="defaultValue">The default value to return if temp is null.</param>
    /// <returns>Temperature in Celsius, or the default value if null.</returns>
    public static double FahrenheitToCelsius(this double? temp, double defaultValue = 0)
        => temp?.FahrenheitToCelsius() ?? defaultValue;

    /// <summary>
    /// Converts Celsius to Fahrenheit.
    /// </summary>
    /// <param name="temp">The temperature in Celsius.</param>
    /// <returns>Temperature in Fahrenheit, rounded to 1 decimal place.</returns>
    public static double CelsiusToFahrenheit(this double temp)
        => Math.Round((temp * CelsiusToFahrenheitRatio) + FahrenheitOffset, 1);

    /// <summary>
    /// Converts Celsius to Fahrenheit with a default value for null.
    /// </summary>
    /// <param name="temp">The temperature in Celsius.</param>
    /// <param name="defaultValue">The default value to return if temp is null.</param>
    /// <returns>Temperature in Fahrenheit, or the default value if null.</returns>
    public static double CelsiusToFahrenheit(this double? temp, double defaultValue = 0)
        => temp?.CelsiusToFahrenheit() ?? defaultValue;

    #endregion

    #region Wind Speed Conversion

    /// <summary>
    /// Converts wind speed from km/h to the specified unit.
    /// </summary>
    /// <param name="speed">The wind speed in km/h.</param>
    /// <param name="windSpeedUnit">The target wind speed unit.</param>
    /// <returns>Converted wind speed, or 0 if unit is null.</returns>
    public static double ConvertWindSpeed(this double speed, WindSpeedUnits? windSpeedUnit)
        => windSpeedUnit switch
        {
            WindSpeedUnits.KmHr => speed,
            WindSpeedUnits.Mph => speed * KmhToMph,
            WindSpeedUnits.FtSec => speed * KmhToFeetPerSecond,
            WindSpeedUnits.MSec => speed * KmhToMetersPerSecond,
            WindSpeedUnits.Knots => speed * KmhToKnots,
            _ => 0
        };

    /// <summary>
    /// Converts wind speed from km/h to the specified unit with null handling.
    /// </summary>
    /// <param name="speed">The wind speed in km/h.</param>
    /// <param name="windSpeedUnit">The target wind speed unit.</param>
    /// <returns>Converted wind speed, or 0 if speed is null.</returns>
    public static double ConvertWindSpeed(this double? speed, WindSpeedUnits? windSpeedUnit)
        => speed?.ConvertWindSpeed(windSpeedUnit) ?? 0;

    #endregion

    #region Temperature Formatting

    /// <summary>
    /// Formats a temperature value with optional unit symbol.
    /// </summary>
    /// <param name="temperature">The temperature in Celsius.</param>
    /// <param name="unit">The unit to display in (Celsius or Fahrenheit).</param>
    /// <param name="includeUnit">If true, includes the unit symbol (°C or °F).</param>
    /// <returns>Formatted temperature string.</returns>
    public static string FormatTemperature(this double temperature, TemperatureUnits? unit, bool includeUnit = false)
    {
        return unit switch
        {
            null => string.Empty,
            TemperatureUnits.Celsius => includeUnit
                ? $"{NormalizeZero(temperature):n0}{DegreeSymbol}C"
                : $"{NormalizeZero(temperature):n0}{DegreeSymbol}",
            TemperatureUnits.Fahrenheit => includeUnit
                ? $"{NormalizeZero(temperature.CelsiusToFahrenheit()):n0}{DegreeSymbol}F"
                : $"{NormalizeZero(temperature.CelsiusToFahrenheit()):n0}{DegreeSymbol}",
            _ => string.Empty
        };

        // Prevent "-0" display by normalizing zero values
        static double NormalizeZero(double value) => Math.Round(value) == 0 ? 0 : value;
    }

    /// <summary>
    /// Formats a temperature value with optional unit symbol (nullable overload).
    /// </summary>
    /// <param name="temperature">The temperature in Celsius.</param>
    /// <param name="unit">The unit to display in (Celsius or Fahrenheit).</param>
    /// <param name="includeUnit">If true, includes the unit symbol (°C or °F).</param>
    /// <returns>Formatted temperature string, or empty string if temperature is null.</returns>
    public static string FormatTemperature(this double? temperature, TemperatureUnits? unit, bool includeUnit = false)
        => temperature?.FormatTemperature(unit, includeUnit) ?? string.Empty;

    #endregion

    #region Length Formatting

    /// <summary>
    /// Formats a length value with optional unit symbol.
    /// </summary>
    /// <param name="value">The length value in millimeters.</param>
    /// <param name="unit">The unit to display in (Metric or Imperial).</param>
    /// <param name="includeUnit">If true, includes the unit symbol (mm or in).</param>
    /// <returns>Formatted length string, or empty string if value is null.</returns>
    public static string FormatLength(this double? value, DistanceUnits unit, bool includeUnit = false)
    {
        if (!value.HasValue) return string.Empty;

        return unit switch
        {
            DistanceUnits.Metric => includeUnit 
                ? $"{value:n2} mm" 
                : $"{value:n2}",
            DistanceUnits.Imperial => includeUnit 
                ? $"{value * MillimetersToInches:n2} in" 
                : $"{value * MillimetersToInches:n2}",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Invalid distance unit.")
        };
    }

    #endregion

    #region Pressure Formatting

    /// <summary>
    /// Formats a pressure value with optional unit symbol.
    /// </summary>
    /// <param name="value">The pressure value in hectopascals (hPa).</param>
    /// <param name="unit">The unit to display in (hPa, inHg, or mmHg).</param>
    /// <param name="includeUnit">If true, includes the unit symbol.</param>
    /// <returns>Formatted pressure string, or empty string if value is null.</returns>
    public static string FormatPressure(this double? value, BarometerUnits unit, bool includeUnit = false)
    {
        if (!value.HasValue) return string.Empty;

        return unit switch
        {
            BarometerUnits.HPa => includeUnit 
                ? $"{value:F2} hPa" 
                : $"{value:F2}",
            BarometerUnits.InHg => includeUnit 
                ? $"{value * HectopascalsToInchesOfMercury:F2} inHg" 
                : $"{value * HectopascalsToInchesOfMercury:F2}",
            BarometerUnits.MmHg => includeUnit 
                ? $"{value * HectopascalsToMillimetersOfMercury:F2} mmHg" 
                : $"{value * HectopascalsToMillimetersOfMercury:F2}",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Invalid barometer unit.")
        };
    }

    #endregion
}