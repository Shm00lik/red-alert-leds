using Microsoft.Extensions.Logging.Abstractions;

namespace RedAlertLEDs.Tests;

public class TzofarParsingTests
{
    private const string CitiesJson = """
        {"cities":{
          "תל אביב - מרכז העיר":{"id":1155,"he":"תל אביב - מרכז העיר","en":"Tel Aviv - City Center"},
          "חיפה - מערב":{"id":420,"he":"חיפה - מערב","en":"Haifa - West"}
        },"areas":{}}
        """;

    private readonly Cities _cities;

    public TzofarParsingTests()
    {
        _cities = new Cities(new DataDirectory(Path.GetTempPath()), null!, NullLogger<Cities>.Instance);
        _cities.Load(CitiesJson);
    }

    [Fact]
    public void AlertMapsCityNamesToIds()
    {
        const string message = """
            {"type":"ALERT","data":{"notificationId":"x","time":1761300000,"threat":0,"isDrill":false,
             "cities":["תל אביב - מרכז העיר","מקום לא קיים"]}}
            """;

        var alert = TzofarClient.Parse(message, _cities, NullLogger.Instance);

        Assert.NotNull(alert);
        Assert.Equal(AlertKind.Missiles, alert.Kind);
        Assert.Equal([1155], alert.CityIds);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1761300000).UtcDateTime, alert.Time);
    }

    [Theory]
    [InlineData(5, AlertKind.Uav)]
    [InlineData(3, AlertKind.Unknown)]
    public void AlertThreatTypes(int threat, AlertKind expected)
    {
        var message = $$$"""{"type":"ALERT","data":{"time":1761300000,"threat":{{{threat}}},"cities":[]}}""";

        Assert.Equal(expected, TzofarClient.Parse(message, _cities, NullLogger.Instance)!.Kind);
    }

    [Theory]
    [InlineData(0, AlertKind.EarlyWarning)]
    [InlineData(1, AlertKind.Ended)]
    [InlineData(7, AlertKind.Unknown)]
    public void SystemMessageUsesCityIdsAndStringTime(int instructionType, AlertKind expected)
    {
        var message = $$$"""
            {"type":"SYSTEM_MESSAGE","data":{"time":"1761300000","instructionType":{{{instructionType}}},"citiesIds":[420,1155]}}
            """;

        var alert = TzofarClient.Parse(message, _cities, NullLogger.Instance);

        Assert.NotNull(alert);
        Assert.Equal(expected, alert.Kind);
        Assert.Equal([420, 1155], alert.CityIds);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1761300000).UtcDateTime, alert.Time);
    }

    [Fact]
    public void OtherMessageTypesAreIgnored()
    {
        Assert.Null(TzofarClient.Parse("""{"type":"LISTS_VERSIONS","data":{}}""", _cities, NullLogger.Instance));
    }

    [Fact]
    public void SearchMatchesHebrewAndEnglish()
    {
        Assert.Equal(1155, _cities.Search("תל").Single().Id);
        Assert.Equal(420, _cities.Search("haifa").Single().Id);
        Assert.Empty(_cities.Search("  "));
    }
}
