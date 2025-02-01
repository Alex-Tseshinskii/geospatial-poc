using System.ComponentModel;

namespace Poc.Model;

public enum RunOption
{
    [Description("Mongo 2D")]
    Mongo = 1,
    [Description("Mongo 2Dsphere")]
    MongoSphere = 2,
    [Description("Postgre 2D")]
    PostgreGeometry = 3,
    [Description("Postgre 2Dsphere")]
    PostgreGeography = 4,
    [Description("Memory 2D")]
    Memory = 5,
    [Description("Memory 2D with Cached Geofences")]
    MemoryWithCache = 6
}