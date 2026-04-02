using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using RedAlertLEDs.Repositories;
using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.Polygons;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController(
    StateManagerService stateManagerService,
    LedStripService ledStripService,
    PolygonsRepository polygonsRepository
) : ControllerBase
{
    [HttpPost]
    public void SetState(AlertState state)
    {
        stateManagerService.SetState(state, this);
    }

    [HttpGet]
    public AlertState GetState()
    {
        return stateManagerService.GetState();
    }

    [HttpPost]
    public void SetColor(int red, int green, int blue)
    {
        ledStripService.SetColor(Color.FromArgb(red, green, blue), true);
    }

    [HttpPost]
    public void SetColorMultiplier(double multiplier)
    {
        ledStripService.SetColorMultiplier(multiplier);
    }
    
    [HttpGet]
    public double GetColorMultiplier()
    {
        return ledStripService.GetColorMultiplier();
    }

    [HttpPost]
    public void AddPolygon(string polygon)
    {
        polygonsRepository.AddPolygon(polygon);
    }

    [HttpGet]
    public List<string> GetPolygons()
    {
        return polygonsRepository.GetPolygons();
    }
}