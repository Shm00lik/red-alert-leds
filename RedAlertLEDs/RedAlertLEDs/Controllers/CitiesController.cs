using Microsoft.AspNetCore.Mvc;

namespace RedAlertLEDs.Controllers;

public record CityResponse(int Id, string Name, string NameEn);

[ApiController]
[Route("api/[controller]")]
public class CitiesController(Cities cities) : ControllerBase
{
    [HttpGet]
    public IEnumerable<CityResponse> Search([FromQuery] string? q) =>
        cities.Search(q ?? "").Select(c => new CityResponse(c.Id, c.Name, c.NameEn));
}
