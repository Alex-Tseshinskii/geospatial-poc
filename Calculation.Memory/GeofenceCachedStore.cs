using Calculation.PostgreSql.Database;
using Microsoft.Extensions.DependencyInjection;
using Poc.Model;

namespace Calculation.Memory;

public class GeofenceCachedStore : GeofenceStore
{
    private GeofenceEntity[]? _cachedGeofences;

    public GeofenceCachedStore([FromKeyedServices(RunOption.PostgreGeometry)]IGeofenceStore postgresGeofenceStore,
        GeospatialDbContext dbContext) : base(postgresGeofenceStore, dbContext)
    {
    }

    protected override async Task<IEnumerable<GeofenceEntity>> GetAllGeofences()
    {
        _cachedGeofences ??= (await base.GetAllGeofences()).ToArray();

        return _cachedGeofences;
    }

    protected override Task<IEnumerable<GeofenceEntity>> GetAllCircularGeofences() => GetAllGeofences();
}