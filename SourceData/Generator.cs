using System.Drawing;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using SourceData.Model;
using Point = GeoJSON.Net.Geometry.Point;

namespace SourceData;

public class Generator
{
    private const uint DefaultItemCount = 100;
    private const int PrecisionMultiplier = 100;

    /* Continental USA rectangle edges (approximate) */
    private static readonly Rectangle CoordinateSystemEnvelope = new Rectangle(-125, 28, 57, 21);
    private static readonly Random Random = Random.Shared;

    public FeatureCollection Generate(SourceDataType type, SourcesOptions options)
    {
        var count = options.AutoGenerateCount ?? DefaultItemCount;

        var featureCollection = new FeatureCollection();
        for (var i = 0; i < count; i++)
        {
            featureCollection.Features.Add(GenerateFeature(type, options, i));
        }

        return featureCollection;
    }

    private static Feature GenerateFeature(SourceDataType type, SourcesOptions options, int index)
    {
        IGeometryObject geometry = type switch
        {
            SourceDataType.Point => GeneratePoint(),
            SourceDataType.Polygon => GeneratePolygon(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return new Feature(geometry, GenerateProperties(options, index));
    }

    private static Point GeneratePoint() => new(GeneratePosition());

    private static Polygon GeneratePolygon()
    {
        const int maxVerticeCount = 7;
        var verticeCount = Random.Next(3, maxVerticeCount + 1);
        var center = GeneratePosition();

        return new Polygon([ new LineString(GenerateStarConvex(center, verticeCount)) ]);
    }

    private static IPosition GeneratePosition() => new Position(RandomLatitude(), RandomLongitude());

    private static IPosition[] GenerateStarConvex(IPosition center, int count)
    {
        const double meanRadius = 0.003; // 330 meters
        const double radiusDeviation = 0.0005; // 110 meters

        const double angleDeviation = 2 * Math.PI * 15.0 / 360.0; // 15 degrees

        var vertices = new IPosition[count + 1];

        double collectedAngle = 0.0;
        double meanAngle = 2 * Math.PI / count;

        for (int i = 0; i < count; i++)
        {
            double angle = meanAngle + (Random.NextDouble() - 0.5) * angleDeviation;
            double radius = meanRadius + (Random.NextDouble() - 0.5) * radiusDeviation;

            collectedAngle += angle;
            if (i < count - 1)
            {
                meanAngle = (2 * Math.PI - collectedAngle) / (count - i - 1);
            }

            vertices[i] = FromPolarToCartesian(center, radius, collectedAngle);
        }
        /* To close polygon shape line */
        vertices[count] = vertices[0];

        return vertices;
    }

    private static Position FromPolarToCartesian(IPosition center, double radius, double angle)
    {
        return new Position(center.Latitude + radius * Math.Cos(angle), center.Longitude + radius * Math.Sin(angle));
    }

    private static Dictionary<string, object> GenerateProperties(SourcesOptions options, int index) =>
        new()
        {
            { options.CenterNameProperty, $"Name{index}" },
            { options.CenterRegionProperty, $"Region{index}" },
        };

    private static double RandomLatitude() =>
        // ReSharper disable once PossibleLossOfFraction
        Random.Next(CoordinateSystemEnvelope.Y * PrecisionMultiplier, (CoordinateSystemEnvelope.Y + CoordinateSystemEnvelope.Height) * PrecisionMultiplier)
        / PrecisionMultiplier;

    private static double RandomLongitude() =>
        // ReSharper disable once PossibleLossOfFraction
        Random.Next(CoordinateSystemEnvelope.X * PrecisionMultiplier, (CoordinateSystemEnvelope.X + CoordinateSystemEnvelope.Width) * PrecisionMultiplier)
        / PrecisionMultiplier;
}