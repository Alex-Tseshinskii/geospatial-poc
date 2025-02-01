using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poc.Library;
using Poc.Model;
using Poc.Model.Configuration;
using SimpleMaps;
using SourceData.Model;

namespace Polygon.Poc.Batch;

public class OneRunService(RunOption runOption, bool createIndex)
{
    private int _sourceCount;
    private int _matchCount;
    private int _mismatchCount;
    private long _searchTimeTicks;

    public async Task RunAsync(int runCount)
    {
        var totalTicks = 0L;
        var totalSourceCount = 0;

        for(int i = 0; i < runCount; i++)
        {
            await RunAsync();

            totalTicks += _searchTimeTicks;
            totalSourceCount += _sourceCount;
        }

        Console.WriteLine($"Matches count: {_matchCount}");
        Console.WriteLine($"Mismatches count: {_mismatchCount}");
        WriteTimeInfo(totalTicks, totalSourceCount);
        Console.WriteLine($"===================================");
    }

    private async Task RunAsync()
    {
        var serviceProvider = DependencyInjection.BuildServiceProvider([], runOption);

        var geofenceStore = serviceProvider.GetRequiredKeyedService<IGeofenceStore>(runOption);
        var sourcesOptions = serviceProvider.GetRequiredService<IOptions<SourcesOptions>>().Value;
        var logicOptions = serviceProvider.GetRequiredService<IOptions<LogicOptions>>().Value;

        await geofenceStore.PrepareAsync(createIndex);
        Console.Write(".");

        await geofenceStore.LoadPolygonsAsync(sourcesOptions);
        Console.Write(".");

        var fileLoader = new SimpleMapsUsCitiesLoader(sourcesOptions.TestPoints);
        Console.Write(".");

        _searchTimeTicks = 0L;
        _matchCount = 0;
        _mismatchCount = 0;
        _sourceCount = 0;

        foreach (var city in fileLoader.Get())
        {
            var findResult = await geofenceStore.FindPolygonsAsync(city);

            foreach (var geofence in findResult.Geofences)
            {
                if (GeofenceMatcher.Match(city, geofence.Name, logicOptions.MatchRule))
                {
                    _matchCount++;
                }
                else
                {
                    _mismatchCount++;
                }
            }

            _searchTimeTicks += findResult.SearchTime.Ticks;
            _sourceCount++;

            if (_sourceCount % 1000 == 0)
            {
                Console.Write(".");
            }
        }

        Console.WriteLine();
        WriteTimeInfo(_searchTimeTicks, _sourceCount);
    }

    private void WriteTimeInfo(long ticks, int sourceCount)
    {
        if (sourceCount > 0)
        {
            Console.WriteLine($"Total ticks {ticks}, average search time: {new TimeSpan(ticks: ticks/sourceCount)} seconds");
        }
    }
}