using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedAlertLEDs;

public record City(int Id, string Name, string NameEn);

/// <summary>
/// The official Tzofar city list. ALERT messages name cities in Hebrew, SYSTEM_MESSAGE uses IDs,
/// so everything is stored and matched by ID.
/// </summary>
public class Cities(DataDirectory dataDirectory, IHttpClientFactory httpClientFactory, ILogger<Cities> logger)
    : IHostedService
{
    private const string Url = "https://www.tzevaadom.co.il/static/cities.json";

    private Dictionary<string, City> _byName = [];
    private Dictionary<int, City> _byId = [];

    private string CachePath => Path.Combine(dataDirectory.Path, "cities.json");

    public int Count => _byId.Count;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? json = null;

        try
        {
            using var http = httpClientFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(20);
            json = await http.GetStringAsync(Url, cancellationToken);
            Load(json);
            await File.WriteAllTextAsync(CachePath, json, cancellationToken);
            logger.LogInformation("Loaded {Count} cities from Tzofar", Count);
            return;
        }
        catch (Exception e) when (e is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(e, "Failed to download the city list, trying the cache");
        }

        try
        {
            if (File.Exists(CachePath))
            {
                Load(await File.ReadAllTextAsync(CachePath, cancellationToken));
                logger.LogInformation("Loaded {Count} cities from cache", Count);
            }
            else
            {
                logger.LogError("No city list available. Alerts with city names cannot be matched");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load the cached city list");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Load(string json)
    {
        var file = JsonSerializer.Deserialize<CitiesFile>(json)
                   ?? throw new InvalidDataException("Empty city list");

        var byName = new Dictionary<string, City>();
        var byId = new Dictionary<int, City>();

        foreach (var (name, entry) in file.Cities)
        {
            var city = new City(entry.Id, entry.He ?? name, entry.En ?? "");
            byName[name] = city;
            byId.TryAdd(entry.Id, city);
        }

        _byName = byName;
        _byId = byId;
    }

    public int? IdForName(string hebrewName) =>
        _byName.TryGetValue(hebrewName.Trim(), out var city) ? city.Id : null;

    public string NameForId(int id) => _byId.TryGetValue(id, out var city) ? city.Name : id.ToString();

    public City? Get(int id) => _byId.GetValueOrDefault(id);

    public IReadOnlyList<City> Search(string query, int max = 20)
    {
        query = query.Trim();

        if (query.Length == 0)
        {
            return [];
        }

        return _byId.Values
            .Select(c => (City: c, Rank: Rank(c, query)))
            .Where(x => x.Rank >= 0)
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.City.Name, StringComparer.Ordinal)
            .Take(max)
            .Select(x => x.City)
            .ToList();
    }

    // 0 = starts with the query, 1 = contains it, -1 = no match.
    private static int Rank(City city, string query)
    {
        if (city.Name.StartsWith(query, StringComparison.Ordinal) ||
            city.NameEn.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (city.Name.Contains(query, StringComparison.Ordinal) ||
            city.NameEn.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return -1;
    }

    private record CitiesFile([property: JsonPropertyName("cities")] Dictionary<string, CityEntry> Cities);

    private record CityEntry(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("he")] string? He,
        [property: JsonPropertyName("en")] string? En);
}
