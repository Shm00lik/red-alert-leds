using Microsoft.AspNetCore.Mvc;

namespace RedAlertLEDs.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedController(AlertEngine engine) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send(LedCommand command)
    {
        if (command.Effect == null || !LedCommand.Effects.Contains(command.Effect))
        {
            return BadRequest($"Effect must be one of: {string.Join(", ", LedCommand.Effects)}");
        }

        if (command.Color != null && !SettingsController.IsColor(command.Color))
        {
            return BadRequest("Color must look like #RRGGBB");
        }

        await engine.SetManualAsync(command);
        return NoContent();
    }

    [HttpPost("state/{state}")]
    public async Task<IActionResult> ForceState(LedState state)
    {
        if (state == LedState.Manual)
        {
            return BadRequest("Use POST /api/led for manual commands");
        }

        await engine.ForceStateAsync(state);
        return NoContent();
    }
}
