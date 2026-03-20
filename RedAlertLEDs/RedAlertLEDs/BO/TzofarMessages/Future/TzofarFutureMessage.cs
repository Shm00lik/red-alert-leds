using System.Text.Json;
using System.Text.Json.Serialization;
using RedAlertLEDs.BO.Alerts;

namespace RedAlertLEDs.BO.TzofarMessages.Future;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TzofarFutureAlertMessage), "ALERT")]
[JsonDerivedType(typeof(TzofarFutureSystemMessageMessage), "SYSTEM_MESSAGE")]
public abstract class TzofarFutureMessage
{
    public required JsonElement Data { get; init; }

    public abstract Alert? ToAlert();
}