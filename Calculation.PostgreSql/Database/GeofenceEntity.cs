using NetTopologySuite.Geometries;

namespace Calculation.PostgreSql.Database;

public class GeofenceEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Region { get; set; }

    public required Polygon Fence { get; set; }

    public Point? Center { get; set; }

    public double? Radius { get; set; }
}