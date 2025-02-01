using GeoJSON.Net.Geometry;

namespace Poc.Model;

public interface ILocatedItem
{
    IPosition Location { get; }
}