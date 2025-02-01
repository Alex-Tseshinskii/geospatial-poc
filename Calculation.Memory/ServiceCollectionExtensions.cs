using Microsoft.Extensions.DependencyInjection;
using Poc.Model;

namespace Calculation.Memory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeospatialMemory(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IGeofenceStore, GeofenceStore>(RunOption.Memory);
        services.AddKeyedSingleton<IGeofenceStore, GeofenceCachedStore>(RunOption.MemoryWithCache);

        return services;
    }
}