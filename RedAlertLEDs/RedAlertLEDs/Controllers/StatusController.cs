using Microsoft.AspNetCore.Mvc;

namespace RedAlertLEDs.Controllers;

public record StatusResponse(
    LedState State,
    DateTimeOffset StateSince,
    DateTimeOffset? CountdownEndsAt,
    bool TzofarConnected,
    bool MqttConnected,
    bool EspOnline);

[ApiController]
[Route("api/[controller]")]
public class StatusController(AlertEngine engine, TzofarClient tzofar, ILedPublisher publisher) : ControllerBase
{
    [HttpGet]
    public StatusResponse Get() => new(
        engine.State,
        engine.StateSince,
        engine.CountdownEndsAt,
        tzofar.Connected,
        publisher.MqttConnected,
        publisher.EspOnline);
}
