using Microsoft.AspNetCore.Mvc;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController(StateManagerService stateManagerService) : ControllerBase
{
    [HttpPost("/SetState")]
    public void SetState(AlertState state)
    {
        stateManagerService.SetState(state, this);
    }
}