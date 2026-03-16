using RedAlertLEDs.BO;

namespace RedAlertLEDs.Services.Polygons;

public class RelevantAlertEventArgs : EventArgs
{
    public Alert Alert { get; init; }
}