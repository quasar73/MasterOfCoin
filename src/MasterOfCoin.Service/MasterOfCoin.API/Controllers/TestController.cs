using System.Diagnostics;
using Lib.Logger.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Route("test")]
public class TestController(ILogger _logger) : Controller
{
    private ActivitySource _activitySource = new("Tests");
    
    [HttpGet]
    public IActionResult HelloWorld()
    {
        var acitivty = _activitySource.StartActivity("test.activity");
        _logger.Critical("Test Critical log", new { ciritcal = true, test = 123 });
        acitivty?.AddEvent(new(nameof(HelloWorld)));
        return Ok("Hello world!");
    }
}