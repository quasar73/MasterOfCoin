using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController(IUserService _userService) : Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var state = await _userService.Authorize(request.Username, request.Password);

        switch (state.Status)
        {
            case LoginStatus.Success:
                return Ok(new LoginResponse(state.Token));
            case LoginStatus.Unauthorized:
                return Unauthorized("Incorrect username or password.");
            default:
                return BadRequest("Something went wrong.");
        }
    }
    
    [HttpPost("registerAndLogin")]
    public async Task<IActionResult> RegisterAndLogin([FromBody] RegisterRequest request)
    {
        var registerStatus = await _userService.Register(new(request.Username, request.Password, request.DisplayedName, request.Email));

        if (registerStatus == RegisterStatus.Unregister)
        {
            return BadRequest("Not registered.");
        }
        
        var state = await _userService.Authorize(request.Username, request.Password);

        switch (state.Status)
        {
            case LoginStatus.Success:
                return Ok(new LoginResponse(state.Token));
            default:
                return BadRequest("Something went wrong.");
        }
    }
}