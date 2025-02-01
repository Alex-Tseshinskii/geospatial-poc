using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace SourceData;

public static class GeoJsonLoader
{
    public static async Task<FeatureCollection> LoadFeaturesAsync(string filePath)
    {
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(fileStream);
        var contents = await streamReader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<FeatureCollection>(contents) ?? throw new JsonException("FeatureCollection cannot be found");
    }
}