using RedAlertLEDs.BO;
using RedAlertLEDs.Services.HomeFrontCommand;

namespace RedAlertLEDs.Services.Polygons;

public class PolygonsService
{
    private List<string> _relevantPolygons = ["תל אביב"];
    private DateTime _lastAlertTime = DateTime.Now;

    public event EventHandler<RelevantAlertEventArgs>? RelevantAlertReceived;

    public void OnAlertReceived(object? sender, AlertEventArgs e)
    {
        if (e.Alert.Date <= _lastAlertTime)
        {
            return;
        }

        var isRelevantAlert = true; //_relevantPolygons.Any(p => e.Alert.Polygon == p);
        
        if (!isRelevantAlert)
        {
            return;
        }
        
        _lastAlertTime = e.Alert.Date;


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