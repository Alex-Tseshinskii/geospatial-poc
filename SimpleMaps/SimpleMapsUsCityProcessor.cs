using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poc.Model;
using Poc.Model.Configuration;
using SourceData.Model;

namespace SimpleMaps;

public class SimpleMapsUsCityProcessor
{
    public static async Task FindEachCityAsync(IServiceProvider serviceProvider,
        Func<UsCity, Task<SearchResult>> cityFinder,
        Func<UsCity, GeofenceDto, LogicOptions, bool>? matchCriteria = null)
    {
        var sourcesOptions = serviceProvider.GetRequiredService<IOptions<SourcesOptions>>().Value;
        var logicOptions = serviceProvider.GetRequiredService<IOptions<LogicOptions>>().Value;
        var fileLoader = new SimpleMapsUsCitiesLoader(sourcesOptions.TestPoints);

        var logger = serviceProvider.GetRequiredService<ILogger<SimpleMapsUsCityProcessor>>();

        var matchesCount = 0;
        var mismatchesCount = 0;
        var notFoundCount = 0;
        var sourceCount = 0;

        long searchTimeTicks = 0L;

        foreach (var city in fileLoader.Get())
        {
            var findResult = await cityFinder(city);

            if (findResult.Geofences.Length == 0)
            {
                notFoundCount++;
            }
            else
            {
                if (matchCriteria == null)
                {
                    matchesCount += findResult.Geofences.Length;
                }
                else
                {
                    foreach (var geofenceName in findResult.Geofences)
                    {
                        if (matchCriteria(city, geofenceName, logicOptions))
                        {
                            matchesCount++;
                        }
                        else
                        {
                            logger.LogTrace("Mismatch: {CityName}, state {CityState}, county {CityCounty} does not match {GeofenceName}",
                                city.Name, city.State, city.County, geofenceName);
                            mismatchesCount++;
                        }
                    }
                }
            }

            searchTimeTicks += findResult.SearchTime.Ticks;
            sourceCount++;

            if (sourceCount % 1000 == 0)
            {
                logger.LogTrace("{Now}: {SourceCount} records processed", DateTime.Now.ToString("mm:ss.fff"), sourceCount);
            }
        }

        Console.WriteLine($"Matches count: {matchesCount}");
        Console.WriteLine($"Mismatches count: {mismatchesCount}");
        Console.WriteLine($"Not matched count: {notFoundCount}");
        if (sourceCount > 0)
        {
            Console.WriteLine($"Average search time: {new TimeSpan(ticks: searchTimeTicks/sourceCount)}");
        }

    }
}