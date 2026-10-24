using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace RedAlertLEDs.Controllers;

public record SettingsResponse(IReadOnlyList<CityResponse> Cities, StateColors Colors, int Brightness);

public record UpdateSettingsRequest(IReadOnlyList<int> CityIds, StateColors Colors, int Brightness);

[ApiController]
[Route("api/[controller]")]
public partial class SettingsController(Settings settings, Cities cities, AlertEngine engine) : ControllerBase
{
    [HttpGet]
    public SettingsResponse Get() => ToResponse(settings.Current);

    [HttpPut]
    public async Task<ActionResult<SettingsResponse>> Update(UpdateSettingsRequest request)
    {
        if (!IsColor(request.Colors.EarlyWarning) || !IsColor(request.Colors.Alert) || !IsColor(request.Colors.Safe))
        {
            return BadRequest("Colors must look like #RRGGBB");
        }

        // Drop IDs that aren't in the official list (when it's loaded) so they can't silently never match.
        var cityIds = cities.Count == 0 ? request.CityIds : request.CityIds.Where(id => cities.Get(id) != null).ToList();

        settings.Update(new SettingsData(cityIds, request.Colors, request.Brightness));
        await engine.ResendCurrentAsync();

        return ToResponse(settings.Current);
    }

    private SettingsResponse ToResponse(SettingsData data) => new(
        data.CityIds.Select(id => cities.Get(id) is { } c
                ? new CityResponse(c.Id, c.Name, c.NameEn)
                : new CityResponse(id, id.ToString(), ""))
            .ToList(),
        data.Colors,
        data.Brightness);

    public static bool IsColor(string? value) => value != null && ColorRegex().IsMatch(value);

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex ColorRegex();
}
