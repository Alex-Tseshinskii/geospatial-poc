using Poc.Model.Configuration;

namespace SimpleMaps;

public static class GeofenceMatcher
{
    public static bool Match(UsCity city, string geofenceName, MatchRule rule) => rule switch
    {
        MatchRule.State => city.State == geofenceName,
        MatchRule.County => city.County == geofenceName,
        _ => false
    };
}