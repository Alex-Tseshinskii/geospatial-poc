using NetTopologySuite.Geometries;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace Utils;

public static class CsTransformation
{
    public const int Wsg84Srid = 4326;
    internal const int UsAlbersConicSrid = 102003;

    private static readonly CoordinateSystemServices UsContinentalMetreCoordinateSystemServices
        = new (
            new Dictionary<int, string>
            {
                [Wsg84Srid] = GeographicCoordinateSystem.WGS84.WKT,
                [UsAlbersConicSrid] = "PROJCS[\"USA_Contiguous_Albers_Equal_Area_Conic\",\n    GEOGCS[\"NAD83\",\n        DATUM[\"North_American_Datum_1983\",\n            SPHEROID[\"GRS 1980\",6378137,298.257222101,\n                AUTHORITY[\"EPSG\",\"7019\"]],\n            AUTHORITY[\"EPSG\",\"6269\"]],\n        PRIMEM[\"Greenwich\",0,\n            AUTHORITY[\"EPSG\",\"8901\"]],\n        UNIT[\"degree\",0.0174532925199433,\n            AUTHORITY[\"EPSG\",\"9122\"]],\n        AUTHORITY[\"EPSG\",\"4269\"]],\n    PROJECTION[\"Albers_Conic_Equal_Area\"],\n    PARAMETER[\"latitude_of_center\",37.5],\n    PARAMETER[\"longitude_of_center\",-96],\n    PARAMETER[\"standard_parallel_1\",29.5],\n    PARAMETER[\"standard_parallel_2\",45.5],\n    PARAMETER[\"false_easting\",0],\n    PARAMETER[\"false_northing\",0],\n    UNIT[\"metre\",1,\n        AUTHORITY[\"EPSG\",\"9001\"]],\n    AXIS[\"Easting\",EAST],\n    AXIS[\"Northing\",NORTH],\n    AUTHORITY[\"ESRI\",\"102003\"]]",
            });

    public static readonly ICoordinateTransformation UsAlbersConicTransformation = UsContinentalMetreCoordinateSystemServices.CreateTransformation(Wsg84Srid, UsAlbersConicSrid);

    private static readonly CoordinateTransformationFactory Factory = new();
    public static ICoordinateTransformation GetUtmTransformation(Point wgs84Point)
    {
        int utmZone = GetUtmZone(wgs84Point.X);

        var utm = ProjectedCoordinateSystem.WGS84_UTM(utmZone, zoneIsNorth: wgs84Point.Y >= 0);
        return Factory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, utm);
    }

    private static int GetUtmZone(double longitude)
    {
        return (int)Math.Floor((longitude + 180) / 6) + 1;
    }
}