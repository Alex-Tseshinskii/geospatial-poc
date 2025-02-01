using Calculation.PostgreSql.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poc.Model;
using Poc.Model.Configuration;
using Utils;
using Utils.NetTopology;

namespace Calculation.PostgreSql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeospatialPostgreSql(this IServiceCollection services, RunOption runOption)
    {
        NetTopologySuite.NtsGeometryServices.Instance = new(
            NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
            DefaultGeometryFactory.PrecisionModel,
            CsTransformation.Wsg84Srid);

        services.AddDbContext<GeospatialDbContext>((sp, ob) =>
        {
            var options = sp.GetRequiredService<IOptions<PostgreOptions>>().Value;
            ob.UseSnakeCaseNamingConvention();
            ob.UseNpgsql(options.ConnectionString, nob => nob.UseNetTopologySuite(geographyAsDefault: runOption == RunOption.PostgreGeography));
        });

        services.AddKeyedScoped<IGeofenceStore, GeofenceStore>(RunOption.PostgreGeometry);
        services.AddKeyedScoped<IGeofenceStore, GeofenceStore>(RunOption.PostgreGeography);

        return services;
    }
}