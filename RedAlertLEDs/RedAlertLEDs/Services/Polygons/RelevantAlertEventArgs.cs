using RedAlertLEDs.BO;

namespace RedAlertLEDs.Services.Polygons;

public class RelevantAlertEventArgs : EventArgs
{
    public HistoricalAlert Alert { get; init; }
}