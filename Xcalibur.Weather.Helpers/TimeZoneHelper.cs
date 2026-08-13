namespace Xcalibur.Weather.Helpers
{
    /// <summary>
    /// Helper class for timezone conversions.
    /// </summary>
    public static class TimeZoneHelper
    {
        /// <summary>
        /// Converts from timezone.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timezone">The timezone.</param>
        /// <returns></returns>
        public static DateTime ConvertFromTimezone(this DateTime? dateTime, string? timezone)
        {
            if (dateTime == null) return DateTime.MinValue;
            return timezone == null ? dateTime.Value : dateTime.Value.ConvertFromTimezone(timezone);
        }

        /// <summary>
        /// Converts from timezone.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timezone">The timezone.</param>
        /// <returns></returns>
        public static DateTime ConvertFromTimezone(this DateTime dateTime, string? timezone)
        {
            if (timezone == null) return dateTime;
            var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTime(dateTime, timezoneInfo);
        }

        /// <summary>
        /// Converts from timezone in UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timezone">The timezone.</param>
        /// <returns></returns>
        public static DateTime ConvertFromTimezoneUtc(this DateTime? dateTime, string? timezone)
        {
            if (dateTime == null) return DateTime.MinValue;
            return timezone == null ? dateTime.Value : dateTime.Value.ConvertFromTimezoneUtc(timezone);
        }

        /// <summary>
        /// Converts from timezone in UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timezone">The timezone.</param>
        /// <returns></returns>
        public static DateTime ConvertFromTimezoneUtc(this DateTime dateTime, string? timezone)
        {
            if (timezone == null) return dateTime;
            var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timezoneInfo);
        }
    }
}
