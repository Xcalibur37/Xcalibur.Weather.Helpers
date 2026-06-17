using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Helpers.Services;

namespace Xcalibur.Weather.Examples
{
    /// <summary>
    /// Example usage of the Weather Alert Service.
    /// </summary>
    public static class WeatherAlertExample
    {
        /// <summary>
        /// Example: Get combined weather alerts from Meteoalarm and NWS.
        /// </summary>
        public static async Task GetCombinedAlertsExample(ILogger logger)
        {
            // Coordinates for Kansas, US (good for testing NWS)
            var latitude = "39.7456";
            var longitude = "-97.0892";

            logger.LogInformation("Fetching combined weather alerts for coordinates ({Latitude}, {Longitude})", 
                latitude, longitude);

            var alerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(latitude, longitude, logger);

            if (alerts is null)
            {
                logger.LogInformation("No active weather alerts found");
                return;
            }

            // Display alert summary
            logger.LogInformation("Region: {Region}", alerts.RegionCode ?? "Unknown");
            logger.LogInformation("Data Sources: {Sources}", string.Join(", ", alerts.DataSources));
            logger.LogInformation("Total Alerts: {Count}", alerts.Alerts.Count);
            logger.LogInformation("Last Updated: {Updated}", alerts.LastUpdated);

            // Display individual alerts
            foreach (var alert in alerts.Alerts)
            {
                logger.LogInformation("----------------------------------------");
                logger.LogInformation("Source: {Source}", alert.Source);
                logger.LogInformation("Event: {Event}", alert.Event ?? "Unknown");
                logger.LogInformation("Severity: {Severity}", alert.Severity ?? "Unknown");
                logger.LogInformation("Urgency: {Urgency}", alert.Urgency ?? "Unknown");
                logger.LogInformation("Headline: {Headline}", alert.Headline ?? "No headline");
                logger.LogInformation("Description: {Description}", 
                    string.IsNullOrWhiteSpace(alert.Description) 
                        ? "No description" 
                        : alert.Description[..Math.Min(200, alert.Description.Length)]);

                if (alert.Onset.HasValue)
                    logger.LogInformation("Onset: {Onset}", alert.Onset.Value);

                if (alert.Expires.HasValue)
                    logger.LogInformation("Expires: {Expires}", alert.Expires.Value);
            }
        }

        /// <summary>
        /// Example: Get Meteoalarm alerts for European location.
        /// </summary>
        public static async Task GetMeteoalarmAlertsExample(ILogger logger)
        {
            // Coordinates for Paris, France
            var latitude = "48.8566";
            var longitude = "2.3522";

            logger.LogInformation("Fetching Meteoalarm alerts for Paris, France");

            var alerts = await WeatherAlertHelper.BuildMeteoalarmAlertsAsync(latitude, longitude, logger);

            if (alerts is null)
            {
                logger.LogInformation("No active Meteoalarm alerts for this location");
                return;
            }

            logger.LogInformation("Found {Count} Meteoalarm alert(s)", alerts.Alerts.Count);

            foreach (var alert in alerts.Alerts)
            {
                logger.LogInformation("Alert: {Event} - Severity: {Severity}", 
                    alert.Event ?? "Unknown", 
                    alert.Severity ?? "Unknown");
            }
        }

        /// <summary>
        /// Example: Get NWS alerts for US location.
        /// </summary>
        public static async Task GetNwsAlertsExample(ILogger logger)
        {
            // Coordinates for Miami, Florida (often has weather alerts)
            var latitude = "25.7617";
            var longitude = "-80.1918";

            logger.LogInformation("Fetching NWS alerts for Miami, Florida");

            var alerts = await WeatherAlertHelper.BuildNwsAlertsAsync(latitude, longitude, logger);

            if (alerts is null)
            {
                logger.LogInformation("No active NWS alerts for this location");
                return;
            }

            logger.LogInformation("Found {Count} NWS alert(s)", alerts.Alerts.Count);

            foreach (var alert in alerts.Alerts)
            {
                logger.LogInformation("Event: {Event}", alert.Event ?? "Unknown");
                logger.LogInformation("Status: {Status}", alert.Status ?? "Unknown");
                logger.LogInformation("Message Type: {Type}", alert.MessageType ?? "Unknown");
                logger.LogInformation("Area: {Area}", alert.AreaDescription ?? "Unknown");

                if (!string.IsNullOrWhiteSpace(alert.Instruction))
                {
                    logger.LogInformation("Instruction: {Instruction}", alert.Instruction);
                }
            }
        }

        /// <summary>
        /// Example: Filter alerts by severity.
        /// </summary>
        public static async Task FilterAlertsBySeverityExample(ILogger logger)
        {
            var latitude = "39.7456";
            var longitude = "-97.0892";

            var alerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(latitude, longitude, logger);

            if (alerts is null)
            {
                logger.LogInformation("No alerts to filter");
                return;
            }

            // Filter for severe or extreme alerts only
            var severeAlerts = alerts.Alerts
                .Where(a => a.Severity == "Severe" || a.Severity == "Extreme")
                .ToList();

            if (severeAlerts.Any())
            {
                logger.LogWarning("Found {Count} severe or extreme alert(s)!", severeAlerts.Count);

                foreach (var alert in severeAlerts)
                {
                    logger.LogWarning("SEVERE ALERT: {Event} - {Headline}", 
                        alert.Event ?? "Unknown", 
                        alert.Headline ?? "No headline");
                }
            }
            else
            {
                logger.LogInformation("No severe or extreme alerts at this time");
            }
        }

        /// <summary>
        /// Example: Check for active alerts and send notification.
        /// </summary>
        public static async Task<bool> CheckForActiveAlertsExample(ILogger logger, string latitude, string longitude)
        {
            var alerts = await WeatherAlertHelper.BuildCombinedAlertsAsync(latitude, longitude, logger);

            if (alerts is null || alerts.Alerts.Count == 0)
            {
                return false;
            }

            // Check for any alerts that are currently effective
            var now = DateTime.UtcNow;
            var activeAlerts = alerts.Alerts
                .Where(a => 
                    (a.Effective == null || a.Effective <= now) && 
                    (a.Expires == null || a.Expires > now))
                .ToList();

            if (activeAlerts.Any())
            {
                logger.LogWarning("ALERT: {Count} active weather alert(s) in effect!", activeAlerts.Count);

                // Here you could trigger a notification system
                foreach (var alert in activeAlerts)
                {
                    logger.LogWarning("  - {Event} (Severity: {Severity})", 
                        alert.Event ?? "Unknown", 
                        alert.Severity ?? "Unknown");
                }

                return true;
            }

            return false;
        }
    }
}
