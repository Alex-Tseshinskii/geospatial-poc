using Calculation.Memory;
using Calculation.Mongo;
using Calculation.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Poc.Model;
using SourceData;

namespace Poc.Library;

public static class DependencyInjection
{
    public static IServiceProvider BuildServiceProvider(string[] commandLineArguments,
        RunOption runOption)
    {
        var configurationService = new ConfigurationService();
        configurationService.Build(commandLineArguments);

        var serviceCollection = new ServiceCollection();
        configurationService.AddConfiguration(serviceCollection);
        serviceCollection.AddLogging(b => configurationService.AddLoggingConfiguration(b));

        serviceCollection
            .AddSourceDataProvider()
            .AddGeospatialPostgreSql(runOption)
            .AddGeospatialMongoDb()
            .AddGeospatialMemory();

        return serviceCollection.BuildServiceProvider();
    }
}