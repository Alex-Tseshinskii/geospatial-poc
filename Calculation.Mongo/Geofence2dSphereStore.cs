using Calculation.Mongo.Database;
using Calculation.Mongo.Model;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Poc.Model;
using Poc.Model.Configuration;
using SourceData;
using SourceData.Helpers;
using SourceData.Model;
using Utils.Benchmark;
using Utils.Buffer;
using Utils.JsonNet;

namespace Calculation.Mongo;

internal class Geofence2dSphereStore : IGeofenceStore
{
    private readonly MongoDb _mongoDb;
    private readonly SourceDataProvider _sourceDataProvider;

    private readonly ILogger<Geofence2dSphereStore> _logger;
    private readonly bool _cacheMaxRadius;

    public Geofence2dSphereStore(MongoDb mongoDb, IOptions<LogicOptions> logicOptions, SourceDataProvider sourceDataProvider, ILogger<Geofence2dSphereStore> logger)
    {
        _mongoDb = mongoDb;
        _sourceDataProvider = sourceDataProvider;
        _logger = logger;
        _cacheMaxRadius = logicOptions.Value.MongoCacheMaxRadius;
    }

    public Task InitializeAsync() => _mongoDb.InitializeAsync();

    public async Task BuildIndexAsync()
    {
        await MongoDb.BuildGeo2DSphereIndexAsync(_mongoDb.GeofencesPolygonSphere, g => g.Fence);
        await MongoDb.BuildGeo2DSphereIndexAsync(_mongoDb.GeofencesCircleSphere, g => g.Center);
    }

    public Task LoadPolygonsAsync(SourcesOptions options) => _sourceDataProvider.ProvideAsync(SourceDataType.Polygon, options, f => SavePolygonAsync(f, options));

    private async Task SavePolygonAsync(Feature feature, SourcesOptions options)
    {
        if (feature.Geometry is not Polygon or MultiPolygon)
        {
            _logger.LogWarning("Feature {Geometry} ignored", feature.Geometry);
            return;
        }

        var vertices = feature.Geometry switch
        {
            Polygon polygon => GetPolygonVertices(polygon),
            MultiPolygon multiPolygon => GetBiggestPolygonVertices(multiPolygon),
            _ => throw new NotImplementedException(),
        };

        var name = feature.Properties[options.PolygonNameProperty].ToString() ?? "<Noname>";
        await _mongoDb.GeofencesPolygonSphere.InsertOneAsync(new GeofencePolygon2dSphere()
        {
            Name = name,
            Fence = Geospatial2dSphere.Polygon(vertices),
        });

        _logger.LogTrace("Polygon '{PolygonName}' added to Mongo DB", name);
    }

    /* Polygon.Coordinates.First() is outer shape of the polygon */
    private static GeoJson2DGeographicCoordinates[] GetPolygonVertices(Polygon polygon) =>
        PolygonHelper.GetPolygonVertices(polygon, CoordinatesByPosition);

    private static GeoJson2DGeographicCoordinates[] GetBiggestPolygonVertices(MultiPolygon multiPolygon) =>
        PolygonHelper.GetBiggestPolygonVertices(multiPolygon, CoordinatesByPosition);

    private static GeoJson2DGeographicCoordinates CoordinatesByPosition(IPosition p) => new GeoJson2DGeographicCoordinates(p.Longitude, p.Latitude);

    public Task LoadCircleCentersAsync(SourcesOptions options) => _sourceDataProvider.ProvideAsync(SourceDataType.Point, options, f => SaveCircleCenterAsync(f, options));

    private async Task SaveCircleCenterAsync(Feature feature, SourcesOptions options)
    {
        if (feature.Geometry is not Point point)
        {
            _logger.LogWarning("Feature {Geometry} ignored", feature.Geometry);
            return;
        }

        var name = feature.Properties[options.CenterNameProperty].ToString() ?? "<Noname>";
        var region = feature.Properties[options.CenterRegionProperty].ToString() ?? "<Noname>";

        /*await _mongoDb.GeofencesCircleSphere.InsertOneAsync(new GeofenceCircle2dSphere()
        {
            Name = name,
            Region = region,
            Center = Geospatial2dSphere.Point(point),
            Radius = RadiusGenerator.Deterministic(name, options)
        });*/
        var polygon = CircleConverter.BuildCircleBuffer(new NetTopologySuite.Geometries.Point(point.Coordinates.Longitude, point.Coordinates.Latitude),
            RadiusGenerator.Deterministic(name, options));

        await _mongoDb.GeofencesPolygonSphere.InsertOneAsync(new GeofencePolygon2dSphere()
        {
            Name = name,
            Fence = Geospatial2dSphere.Polygon(GetPolygonCoordinates(polygon)),
        });

        _logger.LogTrace("Circle center '{CenterName}' added to Mongo DB", name);
    }

    private GeoJson2DGeographicCoordinates[] GetPolygonCoordinates(NetTopologySuite.Geometries.Polygon polygon)
    {
        return polygon.Shell.Coordinates
            .Select(c => new GeoJson2DGeographicCoordinates(c.X, c.Y))
            .ToArray();
    }

    public Task<SearchResult> FindPolygonsAsync(ILocatedItem item) =>
        BenchmarkDatabaseSearchAsync(item, GeoFindIntersectAsync);

    private async Task<GeofenceDto[]> GeoFindIntersectAsync(GeoJsonPoint<GeoJson2DGeographicCoordinates> point)
    {
        var stateFilter = Builders<GeofencePolygon2dSphere>.Filter.GeoIntersects(g => g.Fence, point);
        var findResults = await _mongoDb.GeofencesPolygonSphere.Find(stateFilter).ToListAsync();

        return findResults.Select(g => new GeofenceDto(g.Name)).ToArray();
    }

    public Task<SearchResult> FindCirclesAsync(ILocatedItem item) =>
        // BenchmarkDatabaseSearchAsync(item, GeoFindNearAsync);
        BenchmarkDatabaseSearchAsync(item, GeoFindIntersectAsync);

    private static async Task<SearchResult> BenchmarkDatabaseSearchAsync(ILocatedItem item,
        Func<GeoJsonPoint<GeoJson2DGeographicCoordinates>, Task<GeofenceDto[]>> databaseSearchFunc)
    {
        var point = Geospatial2dSphere.Point(item.Location);

        var (geofences, searchTime) = await StopwatchUtils.ExecuteAndMeasureAsync(() => databaseSearchFunc(point));

        return new(geofences, searchTime);
    }

    private async Task<GeofenceDto[]> GeoFindNearAsync(GeoJsonPoint<GeoJson2DGeographicCoordinates> point)
    {
        var maxRadius = _cacheMaxRadius ? await CacheMaxRadiusAsync() : await GetMaxRadiusAsync();

        var pipeline = new List<BsonDocument>
        {
            new GeoNearStage()
                .WithNear(Geospatial2dSphere.BsonPoint(point.Coordinates))
                .WithDistanceField(nameof(GeofenceCircle2dSphereDistanceResponse.Distance))
                .WithMaxRadius(Geospatial2d.DistanceRadians(maxRadius))
                .AsBson()
        };

        var findResults = await _mongoDb
            .GeofencesCircleSphere
            .Aggregate<GeofenceCircle2dSphereDistanceResponse>(pipeline)
            .ToListAsync();

        return findResults
            .Where(g => g.Distance <= g.Radius)
            .Select(g => new GeofenceDto(g.Name, g.Region))
            .ToArray();
    }

    private double? _maxRadius;
    private async ValueTask<double> CacheMaxRadiusAsync()
    {
        _maxRadius ??= await GetMaxRadiusAsync();

        return _maxRadius.Value;
    }

    private async Task<double> GetMaxRadiusAsync()
    {
        var maxRadiusQuery = await _mongoDb.GeofencesCircleSphere.Aggregate().Group(
            g => BsonNull.Value,
            gr => new
            {
                MaxRadius = gr.Max(g => g.Radius),
            })
            .SingleOrDefaultAsync();

        return maxRadiusQuery?.MaxRadius ?? 0;
    }

    public class GeofenceCircle2dSphereDistanceResponse : GeofenceCircle2dSphere
    {
        public double Distance { get; set; }
    }
}