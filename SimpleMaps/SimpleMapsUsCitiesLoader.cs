using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
// ReSharper disable VirtualMemberCallInConstructor

namespace SimpleMaps;

public sealed class SimpleMapsUsCitiesLoader : IDisposable
{
    private readonly StreamReader _streamReader;
    private readonly CsvReader _csvReader;

    private bool _readStarted;

    public SimpleMapsUsCitiesLoader(string filename)
    {
        _streamReader = new StreamReader(filename, new FileStreamOptions() { Access = FileAccess.Read, Mode = FileMode.Open, });
        _csvReader = new CsvReader(_streamReader, CultureInfo.InvariantCulture);
        _csvReader.Context.RegisterClassMap<UsCityMap>();
    }

    public IEnumerable<UsCity> Get()
    {
        if (!_readStarted)
        {
            StartRead();
        }

        while (_csvReader.Read())
        {
            yield return _csvReader.GetRecord<UsCity>();
        }
    }

    private void StartRead()
    {
        _csvReader.Read();
        _csvReader.ReadHeader();
        _readStarted = true;
    }

    public void Dispose()
    {
        _csvReader.Dispose();
        _streamReader.Dispose();
    }

    private class UsCityMap : ClassMap<UsCity>
    {
        public UsCityMap()
        {
            Map(uc => uc.Name).Name("city");
            Map(uc => uc.State).Name("state_name");
            Map(uc => uc.County).Name("county_name");
            Map(uc => uc.Longitude).Name("lng");
            Map(uc => uc.Latitude).Name("lat");
        }
    }
}