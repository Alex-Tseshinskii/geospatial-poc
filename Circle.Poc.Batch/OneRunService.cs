using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poc.Library;
using Poc.Model;
using SimpleMaps;
using SourceData.Model;

namespace Circle.Poc.Batch;

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
        await geofenceStore.InitializeAsync();
        Console.Write(".");

        if (createIndex)
        {
            await geofenceStore.BuildIndexAsync();
            Console.Write(".");
        }

        var sourcesOptions = serviceProvider.GetRequiredService<IOptions<SourcesOptions>>().Value;
        await geofenceStore.LoadCircleCentersAsync(sourcesOptions);
        Console.Write(".");

        var fileLoader = new SimpleMapsUsCitiesLoader(sourcesOptions.TestPoints);
        Console.Write(".");

        _searchTimeTicks = 0L;
        _matchCount = 0;
        _mismatchCount = 0;
        _sourceCount = 0;

        foreach (var city in fileLoader.Get())
        {
            var findResult = await geofenceStore.FindCirclesAsync(city);

            if (findResult.Geofences.Length == 0)
            {
                _mismatchCount++;
            }
            else
            {
                _matchCount += findResult.Geofences.Length;
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