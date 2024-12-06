using Base.Cache.Contracts;
using MasterOfCoin.API.Options;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(IAuthService _authSerivice) : Controller
{
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return BadRequest("Authorization header is missing.");
        }
        
        var token = authorizationHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token is missing.");
        }

        await _authSerivice.InvalidateToken(token);

        return Ok("Token invalidated.");
    }
}