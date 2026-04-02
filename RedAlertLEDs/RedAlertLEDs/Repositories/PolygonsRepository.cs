using System.Text.Json;
using ILogger = Serilog.ILogger;


namespace RedAlertLEDs.Repositories;

public class PolygonsRepository
{
    private const string DbPath = "polygons.json";

    private List<string> _relevantPolygons = [];
    private bool _usingDb = true;
    
    private readonly ILogger _logger;

    public PolygonsRepository(ILogger logger)
    {
        _logger = logger;
        LoadPolygons();
    }
    
    public List<string> GetPolygons()
    {
        return _relevantPolygons;
    }

    public void AddPolygon(string polygon)
    {
        _relevantPolygons.Add(polygon);

        if (!_usingDb)
        {
            return;
        }
        
        var json = JsonSerializer.Serialize(_relevantPolygons);
        
        File.WriteAllText(DbPath, json);
    }
    
    private void LoadPolygons()
    {
        try
        {
            MakeSureDbExists();

            var json = File.ReadAllText(DbPath);
            var data = JsonSerializer.Deserialize<List<string>>(json);
            _relevantPolygons = data ?? [];
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to load polygons from file!");
            _relevantPolygons = [];
            _usingDb  = false;
        }
    }

    private static void MakeSureDbExists()
    {
        var fullPath = Path.GetFullPath(DbPath);

        var directory = Path.GetDirectoryName(fullPath);

        if (directory == null)
        {
            throw new Exception("Can't find DB directory!");
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(fullPath))
        {
            File.WriteAllText(fullPath, "[]");
        }
    }
}