using SourceData.Model;

namespace SourceData.Helpers;

public static class RadiusGenerator
{
    public static double Deterministic(string name, SourcesOptions options) =>
        Math.Abs(name.GetDeterministicHashCode()) % options.AutoGenerateMinRadius + options.AutoGenerateMinRadius; // Random but deterministic value from MinRadius up to MinRadius * 2 - 1

}