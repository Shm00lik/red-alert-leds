using RedAlertLEDs.BO;
using RedAlertLEDs.Services.HomeFrontCommand;

namespace RedAlertLEDs.Services.Polygons;

public class PolygonsService
{
    private List<string> _relevantPolygons = [];

    private void OnAlertReceived(object? sender, AlertEventArgs e)
    {
        var isRelevantAlert = _relevantPolygons.Any(p => e.Alert.Polygons.Contains(p));

        if (!isRelevantAlert)
        {
            return;
        }
        
        // call a new event
    }
}