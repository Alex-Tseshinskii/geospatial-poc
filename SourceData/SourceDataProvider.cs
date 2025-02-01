using GeoJSON.Net.Feature;
using SourceData.Model;

namespace SourceData;

public class SourceDataProvider
{
    private readonly Generator _generator;

    public SourceDataProvider(Generator generator)
    {
        _generator = generator;
    }

    public async Task ProvideAsync(SourceDataType type, SourcesOptions options,
        Func<Feature, Task> initializeAction)
    {
        FeatureCollection features;

        if (options.AutoGenerate)
        {
            features = _generator.Generate(type, options);
        }
        else
        {
            var sourceFilePath = type switch
            {
                SourceDataType.Polygon => options.Polygons,
                SourceDataType.Point => options.Centers,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };

            features = await GeoJsonLoader.LoadFeaturesAsync(sourceFilePath);
        }

        foreach (var feature in features.Features)
        {
            await initializeAction.Invoke(feature);
        }
    }
}