using Calculation.PostgreSql.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using Poc.Model;
using SourceData.Model;
using Utils.Benchmark;
using Utils.NetTopology;
using static Poc.Model.NetTopologyUtils;
using static Utils.CsTransformation;

namespace Calculation.Memory;

public class GeofenceStore : IGeofenceStore
{
    private readonly IGeofenceStore _postgreGeofenceStore;
    private readonly GeospatialDbContext _dbContext;

    /* NetTopologySuite client-side calculation does not support spheroid so it does not make any sense to test it with something else than Geometry objects */
    public GeofenceStore([FromKeyedServices(RunOption.PostgreGeometry)] IGeofenceStore postgresGeofenceStore,
        GeospatialDbContext dbContext)
    {
        _postgreGeofenceStore = postgresGeofenceStore;
        _dbContext = dbContext;
    }

    public Task InitializeAsync() => _postgreGeofenceStore.InitializeAsync();
    public Task BuildIndexAsync() => _postgreGeofenceStore.BuildIndexAsync();

    public Task LoadPolygonsAsync(SourcesOptions options) => _postgreGeofenceStore.LoadPolygonsAsync(options);

    public Task LoadCircleCentersAsync(SourcesOptions options) => _postgreGeofenceStore.LoadCircleCentersAsync(options);

    protected virtual async Task<IEnumerable<GeofenceEntity>> GetAllGeofences() => await _dbContext.Geofences.ToListAsync();

    protected virtual Task<IEnumerable<GeofenceEntity>> GetAllCircularGeofences() => GetAllGeofences();

    public async Task<SearchResult> FindPolygonsAsync(ILocatedItem item)
    {
        var cityLocation = ItemPoint(item);

        var (found, searchTime) = await StopwatchUtils.ExecuteAndMeasureAsync(async () =>
        {
            var geofences = await GetAllGeofences();
            return geofences.Where(g => g.Fence.Intersects(cityLocation)).Select(g => new GeofenceDto(g.Name)).ToArray();
        });

        return new(found, searchTime);
    }

    public async Task<SearchResult> FindCirclesAsync(ILocatedItem item)
    {
        var cityLocation = ProjectPoint(ItemPoint(item));

        var (found, searchTime) = await StopwatchUtils.ExecuteAndMeasureAsync(async () =>
        {
            var geofences = await GetAllCircularGeofences();
            return geofences
                .Where(g => g.Center!.IsWithinDistance(cityLocation, g.Radius!.Value))
                .Select(g => new GeofenceDto(g.Name, g.Region)).ToArray();

        });

        return new(found, searchTime);
    }

    private static Point ProjectPoint(Point geographicPoint)
    {
        var transformed = UsAlbersConicTransformation.MathTransform.Transform(geographicPoint.Coordinate.X, geographicPoint.Coordinate.Y);
        return DefaultGeometryFactory.UsAlbersConicGeometryFactory.CreatePoint(new Coordinate(transformed.x, transformed.y));
    }
}