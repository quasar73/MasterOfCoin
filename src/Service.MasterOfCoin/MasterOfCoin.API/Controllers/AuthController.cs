using MasterOfCoin.API.ApiContracts.Auth;
using MasterOfCoin.API.Services.Interfaces;
using MasterOfCoin.API.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController(IAuthService _authService, IContractMapper _mapper) : Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var state = await _authService.Authorize(request.Username, request.Password);

        return await HandleLoginState(state);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var registerStatus = await _authService.Register(_mapper.ToRegisterInfo(request));

        if (registerStatus == RegisterStatus.Unregister)
        {
            return BadRequest("User is not registered.");
        }
        
        var state = await _authService.Authorize(request.Username, request.Password);

        switch (state.Status)
        {
            case LoginStatus.Success:
                return Ok(_mapper.ToLoginResponse(state));
            default:
                return BadRequest("Something went wrong.");
        }
    }
    
    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh([FromQuery] string refreshToken)
    {
        var state = await _authService.Refresh(refreshToken);

        return await HandleLoginState(state);
    }

    private Task<IActionResult> HandleLoginState(LoginState state)
    {
        switch (state.Status)
        {
            case LoginStatus.Success:
                return Task.FromResult<IActionResult>(Ok(_mapper.ToLoginResponse(state)));
            case LoginStatus.Unauthorized:
                return Task.FromResult<IActionResult>(Unauthorized("Incorrect username or password."));
            default:
                return Task.FromResult<IActionResult>(BadRequest("Something went wrong."));
        }
    }
}