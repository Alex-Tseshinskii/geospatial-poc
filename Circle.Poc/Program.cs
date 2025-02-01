using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poc.Library;
using Poc.Model;
using SimpleMaps;
using SourceData.Model;

var option = OptionChooseService.ChooseOption();
Console.WriteLine($"You have chosen {option}");
var createIndex = OptionChooseService.ChooseIndexCreation();

var serviceProvider = DependencyInjection.BuildServiceProvider(args, option);

var geofenceStore = serviceProvider.GetRequiredKeyedService<IGeofenceStore>(option);
var sourcesOptions = serviceProvider.GetRequiredService<IOptions<SourcesOptions>>().Value;

await geofenceStore.PrepareAsync(createIndex);
await geofenceStore.LoadCircleCentersAsync(sourcesOptions);

await SimpleMapsUsCityProcessor.FindEachCityAsync(serviceProvider, geofenceStore.FindCirclesAsync);