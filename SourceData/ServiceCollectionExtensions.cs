using Microsoft.Extensions.DependencyInjection;

namespace SourceData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSourceDataProvider(this IServiceCollection services)
    {
        return services
            .AddSingleton<SourceDataProvider>()
            .AddSingleton<Generator>();
    }
}