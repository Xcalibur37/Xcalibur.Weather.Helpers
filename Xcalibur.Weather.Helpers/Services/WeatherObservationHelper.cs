using Microsoft.Extensions.Logging;
using Xcalibur.Weather.Models.Implementation.Observations;
using Xcalibur.Weather.Services.Observation;

namespace Xcalibur.Weather.Helpers.Services;

/// <summary>
/// Helper class for weather observation operations.
/// Provides simplified access to regional weather observation services.
/// </summary>
public static class WeatherObservationHelper
{
    // Shared HttpClient instance for all observation services to optimize resource usage.
    private static HttpClient _sharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    #region Get Observation

    /// <summary>
    /// Gets the nearest weather observation for the specified coordinates.
    /// Automatically selects the appropriate regional service.
    /// </summary>
    /// <param name="latitude">The latitude (as string).</param>
    /// <param name="longitude">The longitude (as string).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Weather observation or null if not available.</returns>
    public static async Task<WeatherObservation?> GetObservationAsync(
        string latitude, string longitude, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (double.TryParse(latitude, out var lat) && double.TryParse(longitude, out var lon))
            return await GetObservationAsync(lat, lon, logger, cancellationToken);
        logger.LogWarning("Invalid latitude or longitude format: {Lat}, {Lon}", latitude, longitude);
        return null;

    }

    /// <summary>
    /// Gets the nearest weather observation for the specified coordinates.
    /// Automatically selects the appropriate regional service.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Weather observation or null if not available.</returns>
    public static async Task<WeatherObservation?> GetObservationAsync(
        double latitude, double longitude, ILogger logger, CancellationToken cancellationToken = default)
    {
        var service = CreateObservationService(logger);
        return await service.GetObservationAsync(latitude, longitude, cancellationToken);
    }

    #endregion

    #region Get Multiple Observations

    /// <summary>
    /// Gets multiple nearby weather observations for the specified coordinates.
    /// </summary>
    /// <param name="latitude">The latitude (as string).</param>
    /// <param name="longitude">The longitude (as string).</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of weather observations.</returns>
    public static async Task<List<WeatherObservation>> GetMultipleObservationsAsync(
        string latitude, string longitude, int maxResults, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (double.TryParse(latitude, out var lat) && double.TryParse(longitude, out var lon))
            return await GetMultipleObservationsAsync(lat, lon, maxResults, logger, cancellationToken);
        logger.LogWarning("Invalid latitude or longitude format: {Lat}, {Lon}", latitude, longitude);
        return [];

    }

    /// <summary>
    /// Gets multiple nearby weather observations for the specified coordinates.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of weather observations.</returns>
    public static async Task<List<WeatherObservation>> GetMultipleObservationsAsync(
        double latitude, double longitude, int maxResults, ILogger logger, CancellationToken cancellationToken = default)
    {
        var service = CreateObservationService(logger);
        return await service.GetMultipleObservationsAsync(latitude, longitude, maxResults, cancellationToken);
    }

    #endregion

    #region Determine Region

    /// <summary>
    /// Determines which weather region the coordinates belong to.
    /// </summary>
    /// <param name="latitude">The latitude (as string).</param>
    /// <param name="longitude">The longitude (as string).</param>
    /// <returns>Weather region.</returns>
    public static Models.Implementation.Observations.WeatherRegion DetermineRegion(string latitude, string longitude) =>
        !double.TryParse(latitude, out var lat) || !double.TryParse(longitude, out var lon)
            ? Models.Implementation.Observations.WeatherRegion.Unknown
            : DetermineRegion(lat, lon);

    /// <summary>
    /// Determines which weather region the coordinates belong to.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <returns>Weather region.</returns>
    public static Models.Implementation.Observations.WeatherRegion DetermineRegion(double latitude, double longitude) => 
        ObservationRegionResolver.DetermineRegion(latitude, longitude);

    #endregion

    #region Get Specific Regional Service Observation

    /// <summary>
    /// Gets observation specifically from NWS (United States).
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Weather observation or null if not available.</returns>
    public static async Task<WeatherObservation?> GetNwsObservationAsync(
        double latitude, double longitude, ILogger logger, CancellationToken cancellationToken = default)
    {
        var service = new NwsObservationService(_sharedHttpClient, logger);
        return await service.GetObservationAsync(latitude, longitude, cancellationToken);
    }

    /// <summary>
    /// Gets observation specifically from ECCC (Canada).
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Weather observation or null if not available.</returns>
    public static async Task<WeatherObservation?> GetEcccObservationAsync(
        double latitude, double longitude, ILogger logger, CancellationToken cancellationToken = default)
    {
        var service = new EcccObservationService(_sharedHttpClient, logger);
        return await service.GetObservationAsync(latitude, longitude, cancellationToken);
    }

    /// <summary>
    /// Gets observation from METAR (global aviation weather).
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Weather observation or null if not available.</returns>
    public static async Task<WeatherObservation?> GetMetarObservationAsync(
        double latitude, double longitude, ILogger logger, CancellationToken cancellationToken = default)
    {
        var service = new MetarObservationService(_sharedHttpClient, logger);
        return await service.GetObservationAsync(latitude, longitude, cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates a complete weather observation service with all regional services.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <returns>Configured WeatherObservationService.</returns>
    private static WeatherObservationService CreateObservationService(ILogger logger)
    {
        // Create METAR service (used as fallback)
        var metarLogger = logger;
        var metarService = new MetarObservationService(_sharedHttpClient, metarLogger);

        // Create all regional services
        var nwsService = new NwsObservationService(_sharedHttpClient, logger);
        var ecccService = new EcccObservationService(_sharedHttpClient, logger);
        var metOfficeService = new MetOfficeObservationService(_sharedHttpClient, logger, metarService);
        var dwdService = new DwdObservationService(_sharedHttpClient, logger, metarService);
        var meteoFranceService = new MeteoFranceObservationService(_sharedHttpClient, logger, metarService);
        var jmaService = new JmaObservationService(_sharedHttpClient, logger, metarService);
        var bomService = new BomObservationService(_sharedHttpClient, logger, metarService);

        // Create and return orchestrator service
        return new WeatherObservationService(
            logger,
            nwsService,
            ecccService,
            metOfficeService,
            dwdService,
            meteoFranceService,
            jmaService,
            bomService,
            metarService
        );
    }

    #endregion
}
