using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Models.Implementation.WeatherAlerts;
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
    /// Helper class for combined weather alert operations (Meteoalarm + NWS + GDACS + Environment Canada + BOM).
    /// </summary>
    public static class WeatherAlertHelper
    {
        private static HttpClient _sharedHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

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
            if (!double.TryParse(latitude, out var lat) || !double.TryParse(longitude, out var lon))
            {
                logger.LogWarning("Invalid coordinates provided: ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            var region = WeatherRegionHelper.DetermineRegion(lat, lon);
            logger.LogDebug("Detected region: {Region} for coordinates ({Latitude}, {Longitude})", region, latitude, longitude);

            MeteoalarmResponse? meteoalarm = null;
            NwsAlertsResponse? nws = null;
            GdacsResponse? gdacs = null;
            EnvironmentCanadaResponse? envCanada = null;
            BomAlertsResponse? bom = null;
            EmscResponse? emsc = null;
            DwdAlertsResponse? dwd = null;

            var tasks = new List<Task>();

            // Global services - always call these
            tasks.Add(Task.Run(async () => gdacs = await GetGdacsAlertsAsync(latitude, longitude, logger, token), token));
            tasks.Add(Task.Run(async () => emsc = await GetEmscAlertsAsync(latitude, longitude, 500, logger, token), token));

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

            await Task.WhenAll(tasks);

            // Check if we got any results
            if (meteoalarm is null && nws is null && gdacs is null && envCanada is null && bom is null && emsc is null && dwd is null)
            {
                logger.LogDebug("No alerts available from any service for ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            // Construct combined alert information
            return new CombinedWeatherAlertInformation(meteoalarm, nws, gdacs, envCanada, bom, emsc, dwd, latitude, longitude);
        }

        /// <summary>
        /// Builds weather alerts from Meteoalarm only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with Meteoalarm data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildMeteoalarmAlertsAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var meteoalarm = await GetMeteoalarmAlertsAsync(latitude, longitude, logger, token);

            if (meteoalarm is null)
            {
                logger.LogDebug("No Meteoalarm alerts available for ({Latitude}, {Longitude})",
                    latitude, longitude);
                return null;
            }

            return new CombinedWeatherAlertInformation(meteoalarm, null, null, null, null, null, null, latitude, longitude);
        }

        /// <summary>
        /// Builds weather alerts from NWS only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with NWS data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildNwsAlertsAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var nws = await GetNwsAlertsAsync(latitude, longitude, logger, token);

            if (nws is null)
            {
                logger.LogDebug("No NWS alerts available for ({Latitude}, {Longitude})",
                    latitude, longitude);
                return null;
            }

            return new CombinedWeatherAlertInformation(null, nws, null, null, null, null, null, latitude, longitude);
        }

        /// <summary>
        /// Builds weather alerts from GDACS only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Combined weather alert information with GDACS data only.</returns>
        public static async Task<CombinedWeatherAlertInformation?> BuildGdacsAlertsAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var gdacs = await GetGdacsAlertsAsync(latitude, longitude, logger, token);

            if (gdacs is null)
            {
                logger.LogDebug("No GDACS alerts available for ({Latitude}, {Longitude})",
                    latitude, longitude);
                return null;
            }

            return new CombinedWeatherAlertInformation(null, null, gdacs, null, null, null, null, latitude, longitude);
        }

        /// <summary>
        /// Gets alerts from Meteoalarm API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Meteoalarm response or null.</returns>
        private static async Task<MeteoalarmResponse?> GetMeteoalarmAlertsAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetMeteoalarmAlertsAsync(latitude, longitude, token);
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
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetNwsAlertsAsync(latitude, longitude, token);
        }

        /// <summary>
        /// Gets alerts from GDACS API only.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>GDACS response or null.</returns>
        private static async Task<GdacsResponse?> GetGdacsAlertsAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetGdacsAlertsAsync(latitude, longitude, token);
        }

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
            string latitude,
            string longitude,
            string provinceCode,
            ILogger logger,
            CancellationToken token)
        {
            var envCanada = await GetEnvironmentCanadaAlertsAsync(latitude, longitude, provinceCode, logger, token);

            if (envCanada is null)
            {
                logger.LogDebug("No Environment Canada alerts available for ({Latitude}, {Longitude}) in province {ProvinceCode}",
                    latitude, longitude, provinceCode);
                return null;
            }

            return new CombinedWeatherAlertInformation(null, null, null, envCanada, null, null, null, latitude, longitude);
        }

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
            string latitude,
            string longitude,
            string stateCode,
            ILogger logger,
            CancellationToken token)
        {
            var bom = await GetBomAlertsAsync(latitude, longitude, stateCode, logger, token);

            if (bom is null)
            {
                logger.LogDebug("No BOM alerts available for ({Latitude}, {Longitude}) in state {StateCode}",
                    latitude, longitude, stateCode);
                return null;
            }

            return new CombinedWeatherAlertInformation(null, null, null, null, bom, null, null, latitude, longitude);
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
        private static async Task<EnvironmentCanadaResponse?> GetEnvironmentCanadaAlertsAsync(
            string latitude,
            string longitude,
            string provinceCode,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetEnvironmentCanadaAlertsAsync(latitude, longitude, provinceCode, token);
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
            string latitude,
            string longitude,
            string stateCode,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetBomAlertsAsync(latitude, longitude, stateCode, token);
        }

        /// <summary>
        /// Builds earthquake alert information from EMSC (European-Mediterranean Seismological Centre).
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="radiusKm">The search radius in kilometers (default: 500).</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>EMSC alerts response or null.</returns>
        public static async Task<EmscResponse?> BuildEmscAlertsAsync(
            string latitude,
            string longitude,
            int radiusKm,
            ILogger logger,
            CancellationToken token)
        {
            logger.LogInformation("Building EMSC earthquake alerts for ({Latitude}, {Longitude}) within {Radius}km",
                latitude, longitude, radiusKm);

            var emscAlerts = await GetEmscAlertsAsync(latitude, longitude, radiusKm, logger, token);
            return emscAlerts;
        }

        /// <summary>
        /// Builds weather warning information from DWD (Deutscher Wetterdienst - German Weather Service).
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>DWD alerts response or null.</returns>
        public static async Task<DwdAlertsResponse?> BuildDwdAlertsAsync(
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            logger.LogInformation("Building DWD weather warnings for ({Latitude}, {Longitude})",
                latitude, longitude);

            var dwdAlerts = await GetDwdAlertsAsync(latitude, longitude, logger, token);
            return dwdAlerts;
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
        private static async Task<EmscResponse?> GetEmscAlertsAsync(
            string latitude,
            string longitude,
            int radiusKm,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetEmscAlertsAsync(latitude, longitude, radiusKm, token);
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
            string latitude,
            string longitude,
            ILogger logger,
            CancellationToken token)
        {
            var service = new WeatherAlertService(_sharedHttpClient, logger);
            return await service.GetDwdAlertsAsync(latitude, longitude, token);
        }
    }
}

