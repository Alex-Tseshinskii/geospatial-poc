using GeoJSON.Net.Geometry;
using MongoDB.Bson;
using Utils;

namespace Calculation.Mongo.Database;

public static class Geospatial2d
{
    public static double[] Point(Point geoJsonPoint) => Point(geoJsonPoint.Coordinates);

    private static double[] Point(IPosition coordinates) => [coordinates.Longitude, coordinates.Latitude];

    public static BsonArray BsonPoint(IPosition coordinates) => new BsonArray(Point(coordinates));

    public static BsonArray BsonPoint(double longitude, double latitude) => BsonPoint(new Position(latitude, longitude));

    public static double DistanceRadians(double distanceMeters) => distanceMeters / GeoConstants.EarthRadiusMeters;
}