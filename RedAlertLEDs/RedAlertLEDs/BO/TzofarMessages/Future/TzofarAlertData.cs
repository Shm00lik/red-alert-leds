namespace RedAlertLEDs.BO.TzofarMessages.Future;

public record TzofarAlertData
{
    public required int Threat { get; init; }

    public required List<string> Cities { get; init; }

    public required int Time { get; init; }
}