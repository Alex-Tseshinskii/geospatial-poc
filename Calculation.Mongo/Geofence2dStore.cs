using Calculation.Mongo.Database;
using Calculation.Mongo.Model;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Poc.Model;
using Poc.Model.Configuration;
using SourceData;
using SourceData.Helpers;
using SourceData.Model;
using Utils.Benchmark;

namespace Calculation.Mongo;

public class Geofence2dStore : IGeofenceStore
{
    private readonly MongoDb _mongoDb;
    private readonly SourceDataProvider _sourceDataProvider;

    private readonly ILogger<Geofence2dStore> _logger;
    private readonly bool _cacheMaxRadius;

    public Geofence2dStore(MongoDb mongoDb, IOptions<LogicOptions> logicOptions, SourceDataProvider sourceDataProvider, ILogger<Geofence2dStore> logger)
    {
        _mongoDb = mongoDb;
        _sourceDataProvider = sourceDataProvider;
        _logger = logger;
        _cacheMaxRadius = logicOptions.Value.MongoCacheMaxRadius;
    }

    public Task InitializeAsync() => _mongoDb.InitializeAsync();

    public Task BuildIndexAsync()
    {
        var geospatialIndex = new CreateIndexModel<GeofenceCircle2d>(Builders<GeofenceCircle2d>.IndexKeys.Geo2D(g => g.Center));
        return _mongoDb.GeofencesCircle2d.Indexes.CreateOneAsync(geospatialIndex);
    }

    /* 2d does not support Intersect method so we cannot use it for polygons */
    public Task LoadPolygonsAsync(SourcesOptions options) => Task.CompletedTask;

    public Task<SearchResult> FindPolygonsAsync(ILocatedItem item) => Task.FromResult(new SearchResult([], TimeSpan.Zero));

    public Task LoadCircleCentersAsync(SourcesOptions options) => _sourceDataProvider.ProvideAsync(SourceDataType.Point,
        options,
        f => SaveCircleCenterAsync(f, options));

    private async Task SaveCircleCenterAsync(Feature feature, SourcesOptions options)
    {
        if (feature.Geometry is not Point point)
        {
            _logger.LogWarning("Feature {Geometry} ignored", feature.Geometry);
            return;
        }

        var name = feature.Properties[options.CenterNameProperty].ToString() ?? "<Noname>";
        var region = feature.Properties[options.CenterRegionProperty].ToString() ?? "<Noname>";

        await _mongoDb.GeofencesCircle2d.InsertOneAsync(new GeofenceCircle2d()
        {
            Name = name,
            Region = region,
            Center = Geospatial2d.Point(point),
            Radius = RadiusGenerator.Deterministic(name, options),
        });

        _logger.LogTrace("Circle center '{CenterName}' added to Mongo DB", name);
    }

    public async Task<SearchResult> FindCirclesAsync(ILocatedItem item)
    {
        var (geofences, searchTime) = await StopwatchUtils.ExecuteAndMeasureAsync(() => GeoNearFindAsync(item));

        return new(geofences, searchTime);
    }

    private async Task<GeofenceDto[]> GeoNearFindAsync(ILocatedItem item)
    {
        var maxRadius = _cacheMaxRadius ? await CachedMaxRadiusAsync() : await GetMaxRadiusAsync();

        // Build the aggregation pipeline
        var pipeline = new List<BsonDocument>
        {
            new GeoNearStage()
                .WithNear(Geospatial2d.BsonPoint(item.Location))
                .WithDistanceField(nameof(GeofenceCircle2dDistanceResponse.Distance))
                .WithMaxRadius(Geospatial2d.DistanceRadians(maxRadius))
                .AsBson()
        };

        var findResults = await _mongoDb
            .GeofencesCircle2d
            .Aggregate<GeofenceCircle2dDistanceResponse>(pipeline)
            .ToListAsync();

        return findResults
            .Where(g => g.Distance <= Geospatial2d.DistanceRadians(g.Radius))
            .Select(g => new GeofenceDto(g.Name, g.Region))
            .ToArray();
    }

    private double? _cachedMaxRadius;
    private async ValueTask<double> CachedMaxRadiusAsync()
    {
        _cachedMaxRadius ??= await GetMaxRadiusAsync();

        return _cachedMaxRadius.Value;
    }

    private async Task<double> GetMaxRadiusAsync()
    {
        var maxRadiusQuery = await _mongoDb.GeofencesCircle2d.Aggregate().Group(
                g => BsonNull.Value,
                gr => new
                {
                    MaxRadius = gr.Max(g => g.Radius),
                })
            .SingleOrDefaultAsync();

        return maxRadiusQuery?.MaxRadius ?? 0;
    }

    public class GeofenceCircle2dDistanceResponse : GeofenceCircle2d
    {
        public double Distance { get; set; }
    }

}