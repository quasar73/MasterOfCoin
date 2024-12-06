using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SpaceController() : Controller
{
    [HttpGet("get-list")]
    public async Task<IActionResult> GetUserSpaces()
    {
        return Ok("Get spaces");
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateSpace()
    {
        var username = HttpContext.User.Identity!.Name!;
        return Ok();
    }
    
    [HttpGet("delete")]
    public async Task<IActionResult> DeleteSpace()
    {
        return Ok("Delete space");
    }
}