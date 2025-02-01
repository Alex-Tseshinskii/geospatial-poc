using System.Linq.Expressions;
using Calculation.Mongo.Model;
using MongoDB.Driver;

namespace Calculation.Mongo.Database;

public class MongoDb
{
    private const string DatabaseName = "geospatial";
    private const string GeofencePolygonCollectionName = "geofences";
    private const string GeofenceCircle2dCollectionName = "geofencesC2d";
    private const string GeofenceCircle2dSphereCollectionName = "geofencesC2dsphere";

    private readonly IMongoDatabase _database;

    public IMongoCollection<GeofencePolygon2dSphere> GeofencesPolygonSphere { get; }
    public IMongoCollection<GeofenceCircle2d> GeofencesCircle2d { get; }
    public IMongoCollection<GeofenceCircle2dSphere> GeofencesCircleSphere { get; }

    public MongoDb(string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);

        _database = mongoClient.GetDatabase(DatabaseName);

        GeofencesPolygonSphere = _database.GetCollection<GeofencePolygon2dSphere>(GeofencePolygonCollectionName);
        GeofencesCircle2d = _database.GetCollection<GeofenceCircle2d>(GeofenceCircle2dCollectionName);
        GeofencesCircleSphere = _database.GetCollection<GeofenceCircle2dSphere>(GeofenceCircle2dSphereCollectionName);
    }

    public Task InitializeAsync()
    {
        return Task.WhenAll(
            _database.DropCollectionAsync(GeofencePolygonCollectionName),
            _database.DropCollectionAsync(GeofenceCircle2dCollectionName),
            _database.DropCollectionAsync(GeofenceCircle2dSphereCollectionName));
    }

    public static Task BuildGeo2DSphereIndexAsync<T>(IMongoCollection<T> collection, Expression<Func<T, object>> keyField)
    {
        var indexModel = new CreateIndexModel<T>(Builders<T>.IndexKeys.Geo2DSphere(keyField));
        return collection.Indexes.CreateOneAsync(indexModel);
    }
}