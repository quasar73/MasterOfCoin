using Base.Cache.Contracts;
using Lib.Cache.Enums;
using MasterOfCoin.API.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(ICacheStore _cacheStore, IConfiguration _configuration) : Controller
{
    private const int DefaultExpireTime = 60;
    
    [HttpPost]
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
        
        var authOptions = new AuthenticationOptions();
        var authSection = _configuration.GetSection(nameof(AuthenticationOptions));
        authSection.Bind(authOptions);

        await _cacheStore.SetAsync(token, true, TimeSpan.FromMinutes(authOptions.ExpireTimeMinutes ?? DefaultExpireTime));

        return Ok("Token invalidated.");
    }
}