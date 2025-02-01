using GeoJSON.Net.Geometry;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Calculation.Mongo.Database;

public static class Geospatial2dSphere
{
    public static GeoJsonPoint<GeoJson2DGeographicCoordinates> Point(Point geoJsonPoint) => Point(geoJsonPoint.Coordinates);

    public static GeoJsonPoint<GeoJson2DGeographicCoordinates> Point(IPosition coordinates) =>
        new(new GeoJson2DGeographicCoordinates(coordinates.Longitude, coordinates.Latitude));

    public static GeoJsonPolygon<TCoordinate> Polygon<TCoordinate>(IEnumerable<TCoordinate> coordinates)
        where TCoordinate : GeoJsonCoordinates =>
        new(new GeoJsonPolygonCoordinates<TCoordinate>(new GeoJsonLinearRingCoordinates<TCoordinate>(coordinates)));

    public static BsonDocument BsonPoint(GeoJson2DGeographicCoordinates coordinates) => new()
    {
        { "type", "Point" },
        { "coordinates", Geospatial2d.BsonPoint(coordinates.Longitude, coordinates.Latitude) },
    };
}