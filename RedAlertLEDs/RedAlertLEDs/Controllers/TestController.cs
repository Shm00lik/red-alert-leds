using Microsoft.AspNetCore.Mvc;
using RedAlertLEDs.BO;
using RedAlertLEDs.Services.LedStrip;

namespace RedAlertLEDs.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    public static Alert? Alert = new()
    {
        Category = AlertCategory.EarlyWarning,
        Description = "TEST",
        Id = "TEST",
        Title = "TEST",
        Polygons = ["תל אביב"]
    };

    [HttpPost("/EarlyWarning")]
    public void EarlyWarning()
    {
        Alert = new Alert
        {
            Category = AlertCategory.EarlyWarning,
            Description = "TEST",
            Id = "TEST",
            Title = "TEST",
            Polygons = ["תל אביב"]
        };
    }

    [HttpPost("/Missiles")]
    public void Missiles()
    {
        Alert = new Alert
        {
            Category = AlertCategory.Missiles,
            Description = "TEST",
            Id = "TEST",
            Title = "TEST",
            Polygons = ["תל אביב"]
        };
    }

    [HttpPost("/IncidentEnded")]
    public void IncidentEnded()
    {
        Alert = new Alert
        {
            Category = AlertCategory.IncidentEnded,
            Description = "TEST",
            Id = "TEST",
            Title = "TEST",
            Polygons = ["תל אביב"]
        };
    }
}