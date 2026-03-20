using RedAlertLEDs.BO;
using RedAlertLEDs.BO.Alerts;

namespace RedAlertLEDs.Services.Tzofar;

public class AlertEventArgs : EventArgs
{
    public Alert Alert { get; init; }
}