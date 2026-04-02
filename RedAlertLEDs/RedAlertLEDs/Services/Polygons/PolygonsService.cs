using RedAlertLEDs.BO;
using RedAlertLEDs.BO.Alerts;
using RedAlertLEDs.Repositories;
using RedAlertLEDs.Services.Tzofar;

namespace RedAlertLEDs.Services.Polygons;

public class PolygonsService(PolygonsRepository polygonsRepository)
{
    private DateTime _lastAlertTime = DateTime.MinValue;

    public event EventHandler<RelevantAlertEventArgs>? RelevantAlertReceived;

    public void OnAlertReceived(object? sender, AlertEventArgs e)
    {
        if (e.Alert.Time <= _lastAlertTime)
        {
            return;
        }

        if (!IsAlertRelevant(e.Alert))
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

    private bool IsAlertRelevant(Alert alert)
    {
        var relevantPolygons = polygonsRepository.GetPolygons();

        return relevantPolygons.Count == 0 || relevantPolygons.Any(p => alert.Polygons.Contains(p));
    }
}