using System.Text.Json;
using RedAlertLEDs.BO.Alerts;

namespace RedAlertLEDs.BO.TzofarMessages.Future;

public class TzofarFutureSystemMessageMessage : TzofarFutureMessage
{
    public override Alert? ToAlert()
    {
        var data = Data.Deserialize<TzofarSystemMessageData>();

        if (data == null)
        {
            return null;
        }

        var alertDateTime = DateTimeOffset.FromUnixTimeSeconds(int.Parse(data.Time)).LocalDateTime;
        var alertCatrgory = GetThreatAlertCategory(data.InstructionType);

        return new Alert
        {
            Time = alertDateTime,
            Polygons = data.CitiesIds.Select(c => c.ToString()).ToList(),
            Category = alertCatrgory
        };
    }

    private static AlertCategory GetThreatAlertCategory(int threat)
    {
        return threat switch
        {
            0 => AlertCategory.Missiles,
            1 => AlertCategory.Uav,
            _ => AlertCategory.Unknown
        };
    }
}