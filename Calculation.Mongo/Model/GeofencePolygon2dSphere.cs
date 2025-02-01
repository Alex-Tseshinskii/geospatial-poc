using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Calculation.Mongo.Model;

public class GeofencePolygon2dSphere
{
    [BsonId]
    public ObjectId Id { get; set; }

    public required string Name { get; set; }

    public required GeoJsonGeometry<GeoJson2DGeographicCoordinates> Fence { get; set; }
}