using NetTopologySuite.Geometries;

namespace Utils.NetTopology;

public static class DefaultGeometryFactory
{
    public static readonly PrecisionModel PrecisionModel = new PrecisionModel(1000d);

    public static readonly GeometryFactory Wsg84Instance = new(PrecisionModel, CsTransformation.Wsg84Srid);

    public static readonly GeometryFactory UsAlbersConicGeometryFactory = new GeometryFactory(PrecisionModel, CsTransformation.UsAlbersConicSrid);
}