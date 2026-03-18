using RedAlertLEDs.BO;

namespace RedAlertLEDs.Services.HomeFrontCommand;

public class AlertEventArgs : EventArgs
{
    public HistoricalAlert Alert { get; init; }
}