using RedAlertLEDs.BO;
using RedAlertLEDs.BO.Alerts;
using RedAlertLEDs.Services.Tzofar;

namespace RedAlertLEDs.Services.Polygons;

public class PolygonsService
{
    private List<string> _relevantPolygons = ["תל אביב"];
    private DateTime _lastAlertTime = DateTime.MinValue;

    public event EventHandler<RelevantAlertEventArgs>? RelevantAlertReceived;

    public void OnAlertReceived(object? sender, AlertEventArgs e)
    {
        if (e.Alert.Time <= _lastAlertTime)
        {
            return;
        }

        var isRelevantAlert = true; //_relevantPolygons.Any(p => e.Alert.Polygon == p);

        if (!isRelevantAlert)
        {
            return;
        }

        _lastAlertTime = e.Alert.Time;


        OnRelevantAlertReceived(e.Alert);
    }

    private void OnRelevantAlertReceived(Alert alert)
    {
        RelevantAlertReceived?.Invoke(this, new RelevantAlertEventArgs
        {
            Alert = alert
        });
    }
}