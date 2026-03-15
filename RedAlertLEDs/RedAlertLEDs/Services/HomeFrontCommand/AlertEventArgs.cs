using RedAlertLEDs.BO;

namespace RedAlertLEDs.Services.HomeFrontCommand;

public class AlertEventArgs : EventArgs
{
    public Alert Alert { get; init; }
}