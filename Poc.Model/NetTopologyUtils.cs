using GeoJSON.Net.Geometry;
using NetTopologySuite.Geometries;
using Utils.NetTopology;
using Point = NetTopologySuite.Geometries.Point;

namespace Poc.Model;

public static class NetTopologyUtils
{
    public static Point ItemPoint(ILocatedItem item) => PositionPoint(item.Location);

    public static Point PositionPoint(IPosition position) =>
        DefaultGeometryFactory.Wsg84Instance.CreatePoint(new Coordinate(position.Longitude, position.Latitude));
}