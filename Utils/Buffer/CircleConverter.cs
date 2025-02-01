using NetTopologySuite.Geometries;
using Utils.NetTopology;

namespace Utils.Buffer;

public static class CircleConverter
{
    public static Polygon BuildCircleBuffer(Point center, double radius)
    {
        var transform = CsTransformation.GetUtmTransformation(center);
        var inverseTransform = transform.MathTransform.Inverse();

        // Transform point to UTM
        var utmPoint = transform.MathTransform.Transform([ center.X, center.Y ]);

        // Apply buffer
        // TODO select relevant factory
        var bufferedUtm = DefaultGeometryFactory.Wsg84Instance.CreatePoint(new Coordinate(utmPoint[0], utmPoint[1])).Buffer(radius) as Polygon
                          ?? throw new InvalidCastException("Unexpected point buffer type");

        // Convert back to WGS84
        var pointsWgs84 = inverseTransform.TransformList(bufferedUtm.Coordinates.Select(c => new[] { c.X, c.Y }).ToList());
        var coordinatesWgs84 = pointsWgs84.Select(p => new Coordinate(p[0], p[1])).ToArray();

        return DefaultGeometryFactory.Wsg84Instance.CreatePolygon(DefaultGeometryFactory.Wsg84Instance.CreateLinearRing(coordinatesWgs84));
    }
}