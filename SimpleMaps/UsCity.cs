using GeoJSON.Net.Geometry;
using Poc.Model;

namespace SimpleMaps;

public class UsCity : ILocatedItem
{
    private double _latitude;
    private double _longitude;

    private IPosition _position = new Position(0, 0);

    private void UpdatePosition()
    {
        _position = new Position(_latitude, _longitude);
    }
    public required string Name { get; set; }
    public required string State { get; set; }
    public required string County { get; set; }
    public required double Longitude { get => _longitude; set { _longitude = value; UpdatePosition(); } }
    public required double Latitude { get => _latitude; set { _latitude = value; UpdatePosition(); } }

    public override string ToString() => $"Name: {Name}, State: {State}, Longitude: {Longitude}, Latitude: {Latitude}";
    public IPosition Location => _position;
}