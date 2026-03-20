namespace RedAlertLEDs.BO.Alerts;

public record Alert
{
    public required DateTime Time { get; init; }

    public required AlertCategory Category { get; init; }

    public required List<string> Polygons { get; init; }
}