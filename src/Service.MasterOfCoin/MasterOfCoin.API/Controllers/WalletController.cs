using MasterOfCoin.API.ApiContracts.Wallet;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallets.Contracts.Interfaces;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WalletController(
    IValidationService _validationService,
    IWalletsApi _walletsApi, 
    IContractMapper _mapper) : Controller
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, request.SpaceId);

        if (!valid) return BadRequest("User doesn't have provided space.");
        
        var apiRequest = _mapper.ToApiCreateWalletRequest(request);
        var result = await _walletsApi.CreateWallet(apiRequest);

        return Ok(_mapper.ToCreateWalletResponse(result, apiRequest));
    }
}