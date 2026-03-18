using RedAlertLEDs.BO;
using RedAlertLEDs.Services.HomeFrontCommand;

namespace RedAlertLEDs.Services.Polygons;

public class PolygonsService
{
    private List<string> _relevantPolygons = ["תל אביב"];

    public event EventHandler<RelevantAlertEventArgs>? RelevantAlertReceived;

    public void OnAlertReceived(object? sender, AlertEventArgs e)
    {
        var isRelevantAlert = _relevantPolygons.Any(p => e.Alert.Polygon == p);

        if (!isRelevantAlert)
        {
            return;
        }

        OnRelevantAlertReceived(e.Alert);
    }

    private void OnRelevantAlertReceived(HistoricalAlert alert)
    {
        RelevantAlertReceived?.Invoke(this, new RelevantAlertEventArgs
        {
            Alert = alert
        });
    }
}