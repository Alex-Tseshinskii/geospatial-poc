using Calculation.PostgreSql.Database;
using GeoJSON.Net.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Poc.Model;
using SourceData;
using SourceData.Helpers;
using SourceData.Model;
using Utils.Benchmark;
using Utils.Buffer;
using Utils.JsonNet;
using Utils.NetTopology;
using NetTopology = NetTopologySuite.Geometries;
using JsonNet = GeoJSON.Net.Geometry;

namespace Calculation.PostgreSql;

internal class GeofenceStore : IGeofenceStore
{
    private readonly RunOption _runOption;

    private readonly GeospatialDbContext _dbContext;
    private readonly SourceDataProvider _sourceDataProvider;
    private readonly ILogger<GeofenceStore> _logger;

    public GeofenceStore([ServiceKey] RunOption runOption, GeospatialDbContext dbContext, SourceDataProvider sourceDataProvider, ILogger<GeofenceStore> logger)
    {
        _runOption = runOption;
        _dbContext = dbContext;
        _sourceDataProvider = sourceDataProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        /* Can do that via migration, but defining precise database types requires model tuning */
        await _dbContext.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS geofences");

        var polygonDataType = _runOption == RunOption.PostgreGeography ? "geography(POLYGON, 4326)" : "geometry(POLYGON, 4326)";
        var centerDataType = _runOption == RunOption.PostgreGeography ? "geography(POINT, 4326)" : "geometry(POINT, 4326)";

#pragma warning disable EF1002
        await _dbContext.Database.ExecuteSqlRawAsync($"CREATE TABLE geofences (id SERIAL PRIMARY KEY, name VARCHAR, region VARCHAR, fence {polygonDataType}, center {centerDataType}, radius DOUBLE PRECISION)");
#pragma warning restore EF1002
    }

    public async Task BuildIndexAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync("CREATE INDEX ix_spatial_data ON geofences USING GIST ( fence )");
    }

    public async Task LoadPolygonsAsync(SourcesOptions options)
    {
        await _sourceDataProvider.ProvideAsync(SourceDataType.Polygon, options, feature => AddPolygonAsync(feature, options));

        await _dbContext.SaveChangesAsync();
        _logger.LogTrace("All polygons are saved to PostgreSQL");
    }

    private Task AddPolygonAsync(Feature feature, SourcesOptions options)
    {
        if (feature.Geometry is not JsonNet.Polygon or JsonNet.MultiPolygon)
        {
            _logger.LogWarning("Feature {Geometry} ignored", feature.Geometry);
            return Task.CompletedTask;
        }

        var coordinates = feature.Geometry switch
        {
            JsonNet.Polygon polygon => BuildPolygonVertices(polygon),
            JsonNet.MultiPolygon multiPolygon => BuildBiggestPolygonVertices(multiPolygon),
            _ => throw new NotImplementedException(),
        };

        var name = feature.Properties[options.PolygonNameProperty].ToString() ?? "<Noname>";
        var geofence = new GeofenceEntity()
        {
            Name = name,
            Fence = DefaultGeometryFactory.Wsg84Instance.CreatePolygon(DefaultGeometryFactory.Wsg84Instance.CreateLinearRing(coordinates)),
        };

        _dbContext.Geofences.Add(geofence);
        _logger.LogTrace("Polygon '{PolygonName}' arranged to add to PostgreSQL", name);

        return Task.CompletedTask;
    }

    private static NetTopology.Coordinate[] BuildPolygonVertices(JsonNet.Polygon polygon) =>
        PolygonHelper.GetPolygonVertices(polygon, CoordinatesByPosition);

    private static NetTopology.Coordinate[] BuildBiggestPolygonVertices(JsonNet.MultiPolygon multiPolygon) =>
        PolygonHelper.GetBiggestPolygonVertices(multiPolygon, CoordinatesByPosition);

    private static NetTopology.Coordinate CoordinatesByPosition(JsonNet.IPosition p) => new NetTopology.Coordinate(p.Longitude, p.Latitude);

    public async Task LoadCircleCentersAsync(SourcesOptions options)
    {
        await _sourceDataProvider.ProvideAsync(SourceDataType.Point, options, feature => AddCirclePolygonAsync(feature, options));

        await _dbContext.SaveChangesAsync();
        _logger.LogTrace("All circles are saved to PostgreSQL");
    }

    /* This method approximates circle by polygon and saves result into geospatial data field */
    private Task AddCirclePolygonAsync(Feature feature, SourcesOptions options)
    {
        if (feature.Geometry is not JsonNet.Point point)
        {
            _logger.LogWarning("Feature {Geometry} ignored", feature.Geometry);
            return Task.CompletedTask;
        }

        var name = feature.Properties[options.CenterNameProperty].ToString() ?? "<Noname>";
        var region = feature.Properties[options.CenterRegionProperty].ToString() ?? "<Noname>";

        var geofenceCenter = NetTopologyPoint(point);
        var geofenceRadius = RadiusGenerator.Deterministic(name, options);

        var geofence = new GeofenceEntity()
        {
            Name = name,
            Region = region,
            Fence = CircleConverter.BuildCircleBuffer(geofenceCenter, geofenceRadius),
            Center = geofenceCenter,
            Radius = geofenceRadius,
        };

        _dbContext.Geofences.Add(geofence);
        _logger.LogTrace("Circle '{CircleName}' arranged to add to PostgreSQL, radius {CircleRadius}", name, geofenceRadius);

        return Task.CompletedTask;
    }

    public async Task<SearchResult> FindPolygonsAsync(ILocatedItem item)
    {
        var cityLocation = NetTopologyUtils.ItemPoint(item);

        var (searchResult, searchTime) = await StopwatchUtils.ExecuteAndMeasureAsync(
            () => _dbContext.Geofences.Where(g => g.Fence.Intersects(cityLocation)).ToListAsync());

        return new(searchResult.Select(g => new GeofenceDto(g.Name, g.Region)).ToArray(), searchTime);
    }

    /* Circles are basically polygons here, so search algorithm is the same */
    public Task<SearchResult> FindCirclesAsync(ILocatedItem item) => FindPolygonsAsync(item);

    private NetTopology.Point NetTopologyPoint(JsonNet.Point point) => NetTopologyUtils.PositionPoint(point.Coordinates);
}