using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Calculation.Mongo.Model;

public class GeofenceCircle2dSphere
{
    [BsonId]
    public ObjectId Id { get; set; }

    public required string Name { get; set; }

    public required string Region { get; set; }

    public required GeoJsonPoint<GeoJson2DGeographicCoordinates> Center { get; set; }

    public required double Radius { get; set; }
}