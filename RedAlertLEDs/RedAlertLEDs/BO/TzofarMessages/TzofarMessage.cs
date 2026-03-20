using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RedAlertLEDs.BO.TzofarMessages;

public record TzofarMessage
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("data")]
    public required JsonObject Data { get; init; }
}