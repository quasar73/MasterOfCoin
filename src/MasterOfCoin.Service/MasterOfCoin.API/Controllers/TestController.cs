using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Route("test")]
public class TestController : Controller
{
    private ActivitySource _activitySource = new("Tests");
    
    [HttpGet]
    public IActionResult HelloWorld()
    {
        var acitivty = _activitySource.StartActivity("test.activity");
        acitivty?.AddEvent(new(nameof(HelloWorld)));
        return Ok("Hello world!");
    }
}