using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController(
    StateManagerService stateManagerService, 
    LedStripService ledStripService
) : ControllerBase
{
    [HttpPost]
    public void SetState(AlertState state)
    {
        stateManagerService.SetState(state, this);
    }

    [HttpPost]
    public void SetColor(int red, int green, int blue)
    {
        ledStripService.SetColor(Color.FromArgb(red, green, blue), true);
    }
}