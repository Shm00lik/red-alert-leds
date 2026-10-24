using Microsoft.AspNetCore.Mvc;

namespace RedAlertLEDs.Controllers;

public record AlertResponse(DateTime Time, AlertKind Kind, IReadOnlyList<string> Cities);

[ApiController]
[Route("api/[controller]")]
public class AlertsController(AlertEngine engine, Cities cities) : ControllerBase
{
    [HttpGet]
    public IEnumerable<AlertResponse> Get() =>
        engine.Recent.Select(a => new AlertResponse(a.Time, a.Kind, a.CityIds.Select(cities.NameForId).ToList()));
}
