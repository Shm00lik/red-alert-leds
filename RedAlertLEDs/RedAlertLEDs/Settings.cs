using System.Text.Json;

namespace RedAlertLEDs;

public record StateColors(string EarlyWarning, string Alert, string Safe);

public record SettingsData(IReadOnlyList<int> CityIds, StateColors Colors, int Brightness)
{
    public static SettingsData Default { get; } =
        new([], new StateColors("#FF4500", "#FF0000", "#00FF00"), 255);
}

/// <summary>
/// User settings persisted as JSON in the data directory. An empty city list means "all of Israel".
/// </summary>
public class Settings
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly Lock _lock = new();
    private readonly string? _path;
    private readonly ILogger<Settings>? _logger;
    private SettingsData _data;

    public Settings(DataDirectory dataDirectory, ILogger<Settings> logger)
    {
        _path = Path.Combine(dataDirectory.Path, "settings.json");
        _logger = logger;
        _data = Load();
    }

    /// <summary>In-memory settings, for tests.</summary>
    public Settings(SettingsData data)
    {
        _data = data;
    }

    public SettingsData Current
    {
        get
        {
            lock (_lock)
            {
                return _data;
            }
        }
    }

    public void Update(SettingsData data)
    {
        lock (_lock)
        {
            _data = data with
            {
                CityIds = data.CityIds.Distinct().ToList(),
                Brightness = Math.Clamp(data.Brightness, 0, 255)
            };

            Save();
        }
    }

    private SettingsData Load()
    {
        try
        {
            if (_path != null && File.Exists(_path))
            {
                var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path), JsonOptions);

                if (data != null)
                {
                    return data;
                }
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Failed to read {Path}, using defaults", _path);
        }

        _data = SettingsData.Default;
        Save();
        return _data;
    }

    private void Save()
    {
        if (_path == null)
        {
            return;
        }

        try
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(_data, JsonOptions));
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Failed to write {Path}", _path);
        }
    }
}

public record DataDirectory(string Path);
