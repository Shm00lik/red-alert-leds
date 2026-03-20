using System.Text.Json;
using RedAlertLEDs.BO.Alerts;

namespace RedAlertLEDs.BO.TzofarMessages.Future;

public class TzofarFutureAlertMessage : TzofarFutureMessage
{
    public override Alert? ToAlert()
    {
        var data = Data.Deserialize<TzofarAlertData>();

        if (data == null)
        {
            return null;
        }

        var alertDateTime = DateTimeOffset.FromUnixTimeSeconds(data.Time).LocalDateTime;
        var alertCatrgory = GetThreatAlertCategory(data.Threat);

        return new Alert
        {
            Time = alertDateTime,
            Polygons = data.Cities,
            Category = alertCatrgory
        };
    }

    private static AlertCategory GetThreatAlertCategory(int threat)
    {
        return threat switch
        {
            0 => AlertCategory.Missiles,
            5 => AlertCategory.Uav,
            _ => AlertCategory.Unknown
        };
    }
}