using GeoJSON.Net.Geometry;

namespace Utils.JsonNet;

public static class PolygonHelper
{
    public static T[] GetPolygonVertices<T>(Polygon polygon, Func<IPosition, T> coordinateConverter) =>
        polygon.Coordinates.First() // exterior polygon boundary
         .Coordinates.Select(coordinateConverter)
         .ToArray();

    public static T[] GetBiggestPolygonVertices<T>(MultiPolygon multiPolygon, Func<IPosition, T> coordinateConverter) =>
        GetPolygonVertices(multiPolygon.Coordinates.OrderByDescending(p => p.Coordinates.First().Coordinates.Count).First(), coordinateConverter);
}