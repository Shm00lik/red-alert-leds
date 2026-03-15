using System.Text.Json.Serialization;

namespace RedAlertLEDs.BO;

public record Alert
{
    [JsonPropertyName("id")] public required string Id { get; init; }

    [JsonPropertyName("cat")] public required AlertCategory Category { get; init; }

    [JsonPropertyName("title")] public required string Title { get; init; }

    [JsonPropertyName("data")] public required List<string> Polygons { get; init; }

    [JsonPropertyName("desc")] public required string Description { get; init; }
}