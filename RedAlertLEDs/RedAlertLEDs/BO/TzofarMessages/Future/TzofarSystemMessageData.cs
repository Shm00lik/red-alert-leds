namespace RedAlertLEDs.BO.TzofarMessages.Future;

public record TzofarSystemMessageData
{
    public required int InstructionType { get; init; }

    public required List<int> CitiesIds { get; init; }

    public required string Time { get; init; }
}