namespace SourceData.Model;

public class SourcesOptions
{
    public bool AutoGenerate { get; set; } = false;

    public uint? AutoGenerateCount { get; set; }

    public uint AutoGenerateMinRadius { get; set; } = 50000;

    public required string Polygons { get; init; }

    public required string PolygonNameProperty { get; init;  }

    public required string Centers { get; init; }

    public required string CenterNameProperty { get; init; }

    public required string CenterRegionProperty { get; init; }

    public required string TestPoints { get; init; }
}