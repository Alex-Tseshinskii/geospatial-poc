using SourceData.Model;

namespace Poc.Model;

public interface IGeofenceStore
{
    Task InitializeAsync();

    Task BuildIndexAsync();

    Task LoadPolygonsAsync(SourcesOptions options);

    Task LoadCircleCentersAsync(SourcesOptions options);

    Task<SearchResult> FindPolygonsAsync(ILocatedItem item);

    Task<SearchResult> FindCirclesAsync(ILocatedItem item);

    public async Task PrepareAsync(bool createIndex)
    {
        await InitializeAsync();

        if (createIndex)
        {
            await BuildIndexAsync();
        }
    }
}