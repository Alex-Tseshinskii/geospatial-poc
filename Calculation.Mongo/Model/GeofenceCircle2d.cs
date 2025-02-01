using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Calculation.Mongo.Model;

public class GeofenceCircle2d
{
    [BsonId]
    public ObjectId Id { get; set; }

    public required string Name { get; set; }

    public required string Region { get; set; }

    public required double[] Center { get; set; }

    public required double Radius { get; set; }
}