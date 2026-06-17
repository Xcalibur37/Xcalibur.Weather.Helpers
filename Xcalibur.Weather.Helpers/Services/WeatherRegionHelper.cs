namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for determining weather service regions and locations.
    /// </summary>
    public static class WeatherRegionHelper
    {
        /// <summary>
        /// Determines the weather service region based on coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>The weather region.</returns>
        public static WeatherRegion DetermineRegion(double latitude, double longitude)
        {
            // Canada - check first to avoid overlap with US
            if (latitude >= 41.7 && latitude <= 84.0 && longitude >= -141.0 && longitude <= -52.0)
            {
                return WeatherRegion.Canada;
            }

            // United States (including Alaska and Hawaii)
            if ((latitude >= 24.0 && latitude <= 49.5 && longitude >= -125.0 && longitude <= -66.0) ||  // Continental US
                (latitude >= 18.0 && latitude <= 23.0 && longitude >= -161.0 && longitude <= -154.0) || // Hawaii
                (latitude >= 51.0 && latitude <= 72.0 && longitude >= -169.0 && longitude <= -129.0))   // Alaska
            {
                return WeatherRegion.UnitedStates;
            }

            // Europe (approximate bounds)
            if (latitude >= 35.0 && latitude <= 72.0 && longitude >= -25.0 && longitude <= 45.0)
            {
                return WeatherRegion.Europe;
            }

            // Australia
            if (latitude >= -44.0 && latitude <= -10.0 && longitude >= 113.0 && longitude <= 154.0)
            {
                return WeatherRegion.Australia;
            }

            return WeatherRegion.Other;
        }

        /// <summary>
        /// Determines if coordinates are in Germany.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>True if the coordinates are in Germany; otherwise, false.</returns>
        public static bool IsInGermany(double latitude, double longitude)
        {
            // Germany approximate bounds (exclude Switzerland and Austria)
            return latitude >= 47.3 && latitude <= 55.1 && longitude >= 5.9 && longitude <= 15.0 &&
                   !(latitude <= 48.0 && longitude <= 8.7); // Exclude Switzerland
        }

        /// <summary>
        /// Determines the Canadian province/territory code from coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>Two-letter province/territory code, or null if not in Canada.</returns>
        /// <remarks>
        /// Simplified province determination based on approximate geographic centers.
        /// For production, consider using a more sophisticated reverse geocoding solution.
        /// </remarks>
        public static string? DetermineCanadianProvince(double latitude, double longitude)
        {
            // Prince Edward Island (check first - smallest)
            if (latitude >= 45.5 && latitude <= 47.5 && longitude >= -64.5 && longitude <= -61.5)
                return "pe";

            // New Brunswick (check before Nova Scotia due to overlap)
            if (latitude >= 44.5 && latitude <= 48.0 && longitude >= -69.0 && longitude <= -63.7)
                return "nb";

            // Nova Scotia
            if (latitude >= 43.0 && latitude <= 47.0 && longitude >= -66.5 && longitude <= -59.5)
                return "ns";

            // Newfoundland and Labrador
            if (latitude >= 46.5 && latitude <= 61.0 && longitude >= -67.5 && longitude <= -52.5)
                return "nl";

            // Quebec
            if (latitude >= 45.0 && latitude <= 63.0 && longitude >= -79.5 && longitude <= -57.0)
                return "qc";

            // Ontario
            if (latitude >= 41.0 && latitude <= 57.0 && longitude >= -95.5 && longitude <= -74.0)
                return "on";

            // Manitoba
            if (latitude >= 49.0 && latitude <= 60.0 && longitude >= -102.0 && longitude <= -88.5)
                return "mb";

            // Saskatchewan
            if (latitude >= 49.0 && latitude <= 60.0 && longitude >= -110.0 && longitude <= -101.0)
                return "sk";

            // Alberta
            if (latitude >= 49.0 && latitude <= 60.0 && longitude >= -120.0 && longitude <= -109.5)
                return "ab";

            // British Columbia
            if (latitude >= 48.0 && latitude <= 60.0 && longitude >= -139.0 && longitude <= -114.0)
                return "bc";

            // Yukon
            if (latitude >= 60.0 && latitude <= 70.0 && longitude >= -141.0 && longitude <= -123.0)
                return "yt";

            // Northwest Territories
            if (latitude >= 60.0 && latitude <= 78.0 && longitude >= -136.0 && longitude <= -101.5)
                return "nt";

            // Nunavut
            if (latitude >= 60.0 && latitude <= 84.0 && longitude >= -110.0 && longitude <= -61.0)
                return "nu";

            return null;
        }

        /// <summary>
        /// Determines the Australian state/territory code from coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>Two or three-letter state/territory code, or null if not in Australia.</returns>
        /// <remarks>
        /// Simplified state determination based on approximate geographic bounds.
        /// For production, consider using a more sophisticated reverse geocoding solution.
        /// </remarks>
        public static string? DetermineAustralianState(double latitude, double longitude)
        {
            // Australian Capital Territory (check first - smallest, within NSW)
            if (latitude >= -35.9 && latitude <= -35.1 && longitude >= 148.7 && longitude <= 149.4)
                return "act";

            // Tasmania
            if (latitude >= -44.0 && latitude <= -39.5 && longitude >= 144.0 && longitude <= 149.0)
                return "tas";

            // Victoria
            if (latitude >= -39.5 && latitude <= -33.5 && longitude >= 140.5 && longitude <= 150.0)
                return "vic";

            // New South Wales
            if (latitude >= -37.5 && latitude <= -28.0 && longitude >= 141.0 && longitude <= 154.0)
                return "nsw";

            // Queensland
            if (latitude >= -29.0 && latitude <= -10.0 && longitude >= 138.0 && longitude <= 154.0)
                return "qld";

            // South Australia
            if (latitude >= -38.0 && latitude <= -26.0 && longitude >= 129.0 && longitude <= 141.0)
                return "sa";

            // Northern Territory
            if (latitude >= -26.0 && latitude <= -11.0 && longitude >= 129.0 && longitude <= 138.0)
                return "nt";

            // Western Australia
            if (latitude >= -35.0 && latitude <= -14.0 && longitude >= 113.0 && longitude <= 129.0)
                return "wa";

            return null;
        }
    }
}
