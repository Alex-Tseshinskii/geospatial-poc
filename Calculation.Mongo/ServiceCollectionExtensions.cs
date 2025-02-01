using Calculation.Mongo.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poc.Model;
using Poc.Model.Configuration;

namespace Calculation.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeospatialMongoDb(this IServiceCollection services)
    {
        services.AddSingleton<MongoDb>(sp =>
        {
            var mongoOptions = sp.GetRequiredService<IOptions<MongoOptions>>().Value;

            return new MongoDb(mongoOptions.ConnectionString);
        });

        services.AddKeyedScoped<IGeofenceStore, Geofence2dSphereStore>(RunOption.MongoSphere);
        services.AddKeyedScoped<IGeofenceStore, Geofence2dStore>(RunOption.Mongo);

        return services;
    }
}