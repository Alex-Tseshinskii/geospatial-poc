namespace Poc.Model;

public record SearchResult(GeofenceDto[] Geofences, TimeSpan SearchTime);