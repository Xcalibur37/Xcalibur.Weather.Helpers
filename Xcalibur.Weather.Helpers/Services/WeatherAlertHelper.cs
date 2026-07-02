using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Models.Implementation.WeatherAlerts;
using Xcalibur.Weather.Models.Services.WeatherAlert.Base;
using Xcalibur.Weather.Models.Services.WeatherAlert.Bom;
using Xcalibur.Weather.Models.Services.WeatherAlert.Dwd;
using Xcalibur.Weather.Models.Services.WeatherAlert.Emsc;
using Xcalibur.Weather.Models.Services.WeatherAlert.EnvironmentCanada;
using Xcalibur.Weather.Models.Services.WeatherAlert.Gdacs;
using Xcalibur.Weather.Models.Services.WeatherAlert.Meteoalarm;
using Xcalibur.Weather.Models.Services.WeatherAlert.Nws;
using Xcalibur.Weather.Services;

namespace Xcalibur.Weather.Helpers.Services
{
    /// <summary>
    /// Helper class for combined weather alert operations
    /// Meteoalarm, NWS, GDACS, Environment Canada, BOM, DWD, EMSC).
    /// </summary>
    public static class WeatherAlertHelper
    {
        #region Members

        private static HttpClient _sharedHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        #endregion

        #region Combined Alerts

        /// <summary>
        /// Builds combined weather alert information from all relevant services based on location.
        /// Intelligently selects which services to query based on geographic coordinates.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="provinceCode">Optional: Province/territory code for Canada (e.g., 'on', 'bc'). If null, will be determined from coordinates if in Canada.</param>
        /// <param name="stateCode">Optional: State/territory code for Australia (e.g., 'nsw', 'vic'). If null, will be determined from coordinates if in Australia.</param>
        /// <returns>Combined weather alert information or null if no alerts are available.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildCombinedAlertsAsync(
            string latitude, 
            string longitude, 
            ILogger logger, 
            CancellationToken token, 
            string? provinceCode = null, 
            string? stateCode = null)
        {
            // Validate coordinates
            if (!double.TryParse(latitude, out var lat) || !double.TryParse(longitude, out var lon))
            {
                logger.LogWarning("Invalid coordinates provided: ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            // Determine the region based on coordinates
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);
            logger.LogDebug("Detected region: {Region} for coordinates ({Latitude}, {Longitude})", region, latitude, longitude);

            // Initialize response objects
            MeteoalarmAlertsResponse? meteoalarm = null;
            NwsAlertsResponse? nws = null;
            GdacsAlertsResponse? gdacs = null;
            EnvironmentCanadaAlertsResponse? envCanada = null;
            BomAlertsResponse? bom = null;
            DwdAlertsResponse? dwd = null;
            EmscAlertsResponse? emsc = null;

            // Global services - always call these
            var tasks = new List<Task>
            {
                Task.Run(async () => gdacs = await GetGdacsAlertsAsync(latitude, longitude, logger, token), token),
                Task.Run(async () => emsc = await GetEmscAlertsAsync(latitude, longitude, 500, logger, token), token)
            };

            // Regional services - call based on location
            switch (region)
            {
                case WeatherRegion.UnitedStates:
                    logger.LogDebug("Querying NWS for United States location");
                    tasks.Add(Task.Run(async () => nws = await GetNwsAlertsAsync(latitude, longitude, logger, token), token));
                    break;

                case WeatherRegion.Canada:
                    logger.LogDebug("Querying Environment Canada for Canadian location");
                    var province = provinceCode ?? WeatherRegionHelper.DetermineCanadianProvince(lat, lon);
                    if (!string.IsNullOrEmpty(province))
                    {
                        tasks.Add(Task.Run(async () => envCanada = await GetEnvironmentCanadaAlertsAsync(latitude, longitude, province, logger, token), token));
                    }
                    break;

                case WeatherRegion.Europe:
                    logger.LogDebug("Querying Meteoalarm for European location");
                    tasks.Add(Task.Run(async () => meteoalarm = await GetMeteoalarmAlertsAsync(latitude, longitude, logger, token), token));

                    // If in Germany, also query DWD
                    if (WeatherRegionHelper.IsInGermany(lat, lon))
                    {
                        logger.LogDebug("Querying DWD for German location");
                        tasks.Add(Task.Run(async () => dwd = await GetDwdAlertsAsync(latitude, longitude, logger, token), token));
                    }
                    break;

                case WeatherRegion.Australia:
                    logger.LogDebug("Querying BOM for Australian location");
                    var state = stateCode ?? WeatherRegionHelper.DetermineAustralianState(lat, lon);
                    if (!string.IsNullOrEmpty(state))
                    {
                        tasks.Add(Task.Run(async () => bom = await GetBomAlertsAsync(latitude, longitude, state, logger, token), token));
                    }
                    break;

                case WeatherRegion.Other:
                    logger.LogDebug("Location outside primary service regions, querying global services only");
                    break;
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Check if we got any results
            var alertResponses = new BaseAlertsResponse?[] { meteoalarm, nws, gdacs, envCanada, bom, dwd, emsc };
            if (alertResponses.All(static response => response is null))
            {
                logger.LogDebug("No alerts available from any service for ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            } 

            // Construct combined alert information
            return new CombinedWeatherAlertInformation(meteoalarm, nws, gdacs, envCanada, bom, emsc, dwd, latitude, longitude);
        }

        /// <summary>
        /// Builds combined weather alert information and returns consolidated alerts.
        /// This is a convenience method that fetches alerts and automatically consolidates overlapping alerts
        /// of the same event type, keeping only the highest severity from each group.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="provinceCode">Optional: Province/territory code for Canada (e.g., 'on', 'bc'). If null, will be determined from coordinates if in Canada.</param>
        /// <param name="stateCode">Optional: State/territory code for Australia (e.g., 'nsw', 'vic'). If null, will be determined from coordinates if in Australia.</param>
        /// <returns>
        /// A consolidated list of weather alerts with overlapping duplicates removed.
        /// Returns an empty list if no alerts are available.
        /// </returns>
        /// <example>
        /// <code>
        /// var alerts = await WeatherAlertHelper.BuildCombinedAlertsConsolidatedAsync(
        ///     "39.4300996", "-77.804161", logger, cancellationToken);
        /// 
        /// foreach (var alert in alerts)
        /// {
        ///     Console.WriteLine($"[{alert.Severity}] {alert.Event}");
        /// }
        /// </code>
        /// </example>
        public static async Task<IReadOnlyList<WeatherAlertItem>> BuildCombinedAlertsConsolidatedAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token,
            string? provinceCode = null,
            string? stateCode = null)
        {
            // Get combined alerts
            var combined = await BuildCombinedAlertsAsync(latitude, longitude, logger, token, provinceCode, stateCode);

            // If no alerts are available, return empty list
            if (combined is null || !combined.Alerts.Any())
            {
                logger.LogDebug("No alerts available for consolidation");
                return Array.Empty<WeatherAlertItem>();
            }

            // Consolidate overlapping alerts and return
            return ConsolidateAlerts(combined.Alerts, logger);
        }

        #endregion

        #region Meteoalarm Alerts

        /// <summary>
        /// Builds weather alerts from Meteoalarm only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with Meteoalarm data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildMeteoalarmAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            // Get Meteoalarm alerts for the given coordinates
            var meteoalarm = await GetMeteoalarmAlertsAsync(latitude, longitude, logger, token);

            // If Meteoalarm alerts are found, return them in a CombinedWeatherAlertInformation object
            if (meteoalarm is null)
            {
                logger.LogDebug("No Meteoalarm alerts available for ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            // Return a CombinedWeatherAlertInformation object with only Meteoalarm data populated
            return new CombinedWeatherAlertInformation(meteoalarm, null, null, null, null, null, null, latitude, longitude);

        }

        /// <summary>
        /// Gets alerts from Meteoalarm API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Meteoalarm response or null.</returns>
        private static async Task<MeteoalarmAlertsResponse?> GetMeteoalarmAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetMeteoalarmAlertsAsync(latitude, longitude, token);

        #endregion

        #region NWS Alerts

        /// <summary>
        /// Builds weather alerts from NWS only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with NWS data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildNwsAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            // Get NWS alerts for the given coordinates
            var nws = await GetNwsAlertsAsync(latitude, longitude, logger, token);

            // If NWS alerts are found, return them in a CombinedWeatherAlertInformation object
            if (nws is null)
            {
                logger.LogDebug("No NWS alerts available for ({Latitude}, {Longitude})",
                    latitude, longitude);
                return null;
            }

            // Return a CombinedWeatherAlertInformation object with only NWS data populated
            return new CombinedWeatherAlertInformation(null, nws, null, null, null, null, null, latitude, longitude);

        }

        /// <summary>
        /// Builds weather alerts from NWS and returns consolidated alerts.
        /// This is a convenience method for NWS-only alerts that automatically consolidates
        /// overlapping alerts of the same event type.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>
        /// A consolidated list of NWS weather alerts with overlapping duplicates removed.
        /// Returns an empty list if no alerts are available.
        /// </returns>
        public static async Task<IReadOnlyList<WeatherAlertItem>> BuildNwsAlertsConsolidatedAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            var combined = await BuildNwsAlertsAsync(latitude, longitude, logger, token);

            if (combined is null || !combined.Alerts.Any())
            {
                return Array.Empty<WeatherAlertItem>();
            }

            return ConsolidateAlerts(combined.Alerts, logger);
        }

        /// <summary>
        /// Gets alerts from NWS API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>NWS alerts response or null.</returns>
        private static async Task<NwsAlertsResponse?> GetNwsAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetNwsAlertsAsync(latitude, longitude, token);

        #endregion

        #region GDACS Alerts

        /// <summary>
        /// Builds weather alerts from GDACS only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with GDACS data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildGdacsAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            // Log the start of the GDACS alert building process with the provided coordinates
            var gdacs = await GetGdacsAlertsAsync(latitude, longitude, logger, token);

            // If GDACS alerts are found, return them in a CombinedWeatherAlertInformation object
            if (gdacs is null)
            {
                logger.LogDebug("No GDACS alerts available for ({Latitude}, {Longitude})",
                    latitude, longitude);
                return null;
            }

            // Log that no GDACS alerts were found for the given coordinates
            return new CombinedWeatherAlertInformation(null, null, gdacs, null, null, null, null, latitude, longitude);

        }

        /// <summary>
        /// Gets alerts from GDACS API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>GDACS response or null.</returns>
        private static async Task<GdacsAlertsResponse?> GetGdacsAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetGdacsAlertsAsync(latitude, longitude, token);

        #endregion

        #region Environment Canada Alerts

        /// <summary>
        /// Builds weather alerts from Environment Canada only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="provinceCode">The province/territory code (e.g., 'on', 'bc', 'ab').</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with Environment Canada data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildEnvironmentCanadaAlertsAsync(
            string latitude, string longitude, string provinceCode, ILogger logger, CancellationToken token)
        {
            var envCanada = await GetEnvironmentCanadaAlertsAsync(latitude, longitude, provinceCode, logger, token);

            if (envCanada is not null)
            {
                return new CombinedWeatherAlertInformation(null, null, null, envCanada, null, null, null, latitude,
                    longitude);
            }

            // Log that no Environment Canada alerts were found for the given coordinates and province
            logger.LogDebug("No Environment Canada alerts available for ({Latitude}, {Longitude}) in province {ProvinceCode}",
                latitude, longitude, provinceCode);
            return null;

        }

        /// <summary>
        /// Gets alerts from Environment Canada API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="provinceCode">The province/territory code.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Environment Canada response or null.</returns>
        private static async Task<EnvironmentCanadaAlertsResponse?> GetEnvironmentCanadaAlertsAsync(
            string latitude, string longitude, string provinceCode, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetEnvironmentCanadaAlertsAsync(latitude, longitude, provinceCode, token);

        #endregion

        #region BOM Alerts

        /// <summary>
        /// Builds weather alerts from BOM Australia only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="stateCode">The state/territory code (e.g., 'nsw', 'vic', 'qld').</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with BOM data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildBomAlertsAsync(
            string latitude, string longitude, string stateCode, ILogger logger, CancellationToken token)
        {
            // Get BOM alerts for the given coordinates and state
            var bom = await GetBomAlertsAsync(latitude, longitude, stateCode, logger, token);

            // If BOM alerts are found, return them in a CombinedWeatherAlertInformation object
            if (bom is not null)
            {
                return new CombinedWeatherAlertInformation(null, null, null, null, bom, null, null, latitude,
                    longitude);
            }

            // Log that no BOM alerts were found for the given coordinates and state
            logger.LogDebug("No BOM alerts available for ({Latitude}, {Longitude}) in state {StateCode}",
                latitude, longitude, stateCode);
            return null;

        }

        /// <summary>
        /// Gets alerts from BOM Australia API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="stateCode">The state/territory code.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>BOM alerts response or null.</returns>
        private static async Task<BomAlertsResponse?> GetBomAlertsAsync(
            string latitude, string longitude, string stateCode, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetBomAlertsAsync(latitude, longitude, stateCode, token);

        #endregion

        #region DWD Alerts

        /// <summary>
        /// Builds weather warning information from DWD (Deutscher Wetterdienst - German Weather Service).
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>DWD alerts response or null.</returns>
        public static async Task<DwdAlertsResponse?> BuildDwdAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
        {
            // Log the start of the DWD alert building process with the provided coordinates
            logger.LogInformation("Building DWD weather warnings for ({Latitude}, {Longitude})", latitude, longitude);

            // Call the private method to get DWD alerts and return the result
            return await GetDwdAlertsAsync(latitude, longitude, logger, token);
        }

        /// <summary>
        /// Gets alerts from DWD API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>DWD alerts response or null.</returns>
        private static async Task<DwdAlertsResponse?> GetDwdAlertsAsync(
            string latitude, string longitude, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetDwdAlertsAsync(latitude, longitude, token);

        #endregion

        #region EMSC Alerts

        /// <summary>
        /// Builds earthquake alert information from EMSC (European-Mediterranean Seismological Centre).
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="radiusKm">The search radius in kilometers (default: 500).</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>EMSC alerts response or null.</returns>
        public static async Task<EmscAlertsResponse?> BuildEmscAlertsAsync(
            string latitude, string longitude, int radiusKm, ILogger logger, CancellationToken token)
        {
            // Log the start of the EMSC alert building process with the provided coordinates and radius
            logger.LogInformation("Building EMSC earthquake alerts for ({Latitude}, {Longitude}) within {Radius}km", latitude, longitude, radiusKm);

            // Call the private method to get EMSC alerts and return the result
            return await GetEmscAlertsAsync(latitude, longitude, radiusKm, logger, token);
        }

        /// <summary>
        /// Gets alerts from EMSC API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="radiusKm">The search radius in kilometers.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>EMSC alerts response or null.</returns>
        private static async Task<EmscAlertsResponse?> GetEmscAlertsAsync(
            string latitude, string longitude, int radiusKm, ILogger logger, CancellationToken token)
            => await new WeatherAlertService(_sharedHttpClient, logger).GetEmscAlertsAsync(latitude, longitude, radiusKm, token);

        #endregion

        #region Alert Consolidation

        /// <summary>
        /// Consolidates overlapping alerts by keeping the highest severity alert from each group.
        /// Groups alerts that share the same event type and have overlapping effective periods.
        /// This is useful when weather services issue multiple escalating alerts for the same phenomenon
        /// (e.g., Winter Weather Advisory → Winter Storm Watch → Winter Storm Warning).
        /// </summary>
        /// <param name="alerts">The collection of weather alerts to consolidate.</param>
        /// <param name="logger">Optional logger for diagnostic information.</param>
        /// <returns>A consolidated list of alerts with duplicates removed, keeping the highest severity from each overlapping group.</returns>
        /// <remarks>
        /// This method uses a lightweight algorithm that:
        /// - Groups alerts by event type (e.g., all winter weather alerts together)
        /// - Within each group, identifies alerts with overlapping time periods
        /// - Keeps only the highest severity alert from each overlapping group
        /// - Preserves all other unique alerts
        /// 
        /// Time complexity: O(n²) where n is the number of alerts (typically small, 1-10 alerts)
        /// Space complexity: O(n)
        /// 
        /// Usage:
        /// <code>
        /// var consolidatedAlerts = WeatherAlertHelper.ConsolidateAlerts(
        ///     alerts.Alerts, 
        ///     logger
        /// );
        /// </code>
        /// </remarks>
        public static IReadOnlyList<Models.Implementation.WeatherAlerts.WeatherAlertItem> ConsolidateAlerts(
            IEnumerable<Models.Implementation.WeatherAlerts.WeatherAlertItem> alerts, 
            ILogger? logger = null)
        {
            var alertsList = alerts?.ToList() ?? new List<Models.Implementation.WeatherAlerts.WeatherAlertItem>();

            if (alertsList.Count <= 1)
            {
                return alertsList;
            }

            logger?.LogDebug("Consolidating {Count} alerts", alertsList.Count);

            // Track which alerts to keep (by index)
            var alertsToKeep = new HashSet<int>();

            // Group alerts by event type for efficient comparison
            var eventGroups = alertsList
                .Select((alert, index) => new { Alert = alert, Index = index })
                .GroupBy(x => x.Alert.EventType)
                .ToList();

            foreach (var eventGroup in eventGroups)
            {
                var groupAlerts = eventGroup.ToList();

                // If only one alert in this event type group, keep it
                if (groupAlerts.Count == 1)
                {
                    alertsToKeep.Add(groupAlerts[0].Index);
                    continue;
                }

                // Track which alerts have been processed in this group
                var processedInGroup = new HashSet<int>();

                foreach (var current in groupAlerts)
                {
                    if (processedInGroup.Contains(current.Index))
                    {
                        continue;
                    }

                    // Find all alerts that overlap with this one
                    var overlappingGroup = new List<(Models.Implementation.WeatherAlerts.WeatherAlertItem Alert, int Index)>
                    {
                        (current.Alert, current.Index)
                    };

                    foreach (var other in groupAlerts)
                    {
                        if (other.Index == current.Index || processedInGroup.Contains(other.Index))
                        {
                            continue;
                        }

                        // Check if alerts overlap in time
                        if (AreAlertsOverlapping(current.Alert, other.Alert))
                        {
                            overlappingGroup.Add((other.Alert, other.Index));
                            processedInGroup.Add(other.Index);
                        }
                    }

                    // Keep the highest severity alert from the overlapping group
                    var highestSeverity = overlappingGroup
                        .OrderByDescending(x => GetSeverityRank(x.Alert.Severity))
                        .ThenByDescending(x => x.Alert.Effective) // Most recent if same severity
                        .First();

                    alertsToKeep.Add(highestSeverity.Index);
                    processedInGroup.Add(current.Index);

                    if (overlappingGroup.Count > 1)
                    {
                        logger?.LogDebug(
                            "Consolidated {Count} overlapping '{EventType}' alerts, keeping highest severity: {Severity} - {Event}",
                            overlappingGroup.Count,
                            current.Alert.EventType,
                            highestSeverity.Alert.Severity,
                            highestSeverity.Alert.Event);
                    }
                }
            }

            // Create consolidated list
            var consolidatedAlerts = alertsToKeep
                .OrderBy(x => x)
                .Select(index => alertsList[index])
                .ToList();

            if (consolidatedAlerts.Count < alertsList.Count)
            {
                logger?.LogInformation(
                    "Consolidated {OriginalCount} alerts down to {ConsolidatedCount} unique alerts",
                    alertsList.Count,
                    consolidatedAlerts.Count);
            }
            else
            {
                logger?.LogDebug("No consolidation needed - all alerts are unique");
            }

            return consolidatedAlerts;
        }

        /// <summary>
        /// Determines if two alerts overlap in their effective time periods.
        /// </summary>
        private static bool AreAlertsOverlapping(
            Models.Implementation.WeatherAlerts.WeatherAlertItem alert1,
            Models.Implementation.WeatherAlerts.WeatherAlertItem alert2)
        {
            // If either alert doesn't have time information, assume no overlap
            if (!alert1.Effective.HasValue || !alert2.Effective.HasValue)
            {
                return false;
            }

            var start1 = alert1.Effective.Value;
            var end1 = alert1.Expires ?? DateTime.MaxValue;
            var start2 = alert2.Effective.Value;
            var end2 = alert2.Expires ?? DateTime.MaxValue;

            // Check if time ranges overlap
            return start1 < end2 && start2 < end1;
        }

        /// <summary>
        /// Returns a numeric rank for alert severity (higher = more severe).
        /// </summary>
        private static int GetSeverityRank(Models.Implementation.WeatherAlerts.AlertSeverity severity)
        {
            return severity switch
            {
                Models.Implementation.WeatherAlerts.AlertSeverity.Extreme => 4,
                Models.Implementation.WeatherAlerts.AlertSeverity.Severe => 3,
                Models.Implementation.WeatherAlerts.AlertSeverity.Moderate => 2,
                Models.Implementation.WeatherAlerts.AlertSeverity.Minor => 1,
                _ => 0
            };
        }

        #endregion
    }
}

