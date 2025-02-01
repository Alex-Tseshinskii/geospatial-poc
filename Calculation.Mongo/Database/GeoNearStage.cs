using MongoDB.Bson;

namespace Calculation.Mongo.Database;

public class GeoNearStage
{
    private BsonValue? _nearValue;
    private string? _distanceFieldName;
    private double? _maxRadius;

    public BsonDocument AsBson() =>
        new BsonDocument
        {
            {
                "$geoNear", new BsonDocument
                {
                    { "near", _nearValue },
                    { "distanceField", _distanceFieldName },
                    { "maxDistance", _maxRadius },
                    { "spherical", true }
                }
            }
        };

    public GeoNearStage WithNear(BsonValue nearValue)
    {
        _nearValue = nearValue;

        return this;
    }

    public GeoNearStage WithDistanceField(string fieldName)
    {
        _distanceFieldName = fieldName;

        return this;
    }

    public GeoNearStage WithMaxRadius(double radius)
    {
        _maxRadius = radius;

        return this;
    }
}