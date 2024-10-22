using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Route("test")]
public class TestController : Controller
{
    [HttpGet]
    public IActionResult HelloWorld()
    {
        return Ok("Hello world!");
    }
}