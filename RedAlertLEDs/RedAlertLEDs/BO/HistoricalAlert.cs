using System.Text.Json.Serialization;

namespace RedAlertLEDs.BO;

public record HistoricalAlert
{
    [JsonPropertyName("alertDate")] public required DateTime Date { get; init; }

    [JsonPropertyName("title")] public required string Title { get; init; }

    [JsonPropertyName("data")] public required string Polygon { get; init; }

    [JsonPropertyName("category")] public required HistoricalAlertCategory Category { get; init; }
}