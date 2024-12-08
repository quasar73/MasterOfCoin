using MasterOfCoin.API.ApiContracts.Wallet;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallets.Contracts.Contracts.Enums;
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
    [HttpGet("get-list/{spaceId}")]
    public async Task<IActionResult> GetList([FromRoute] Guid spaceId)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");

        var result = await _walletsApi.GetWallets(new(spaceId));

        if (result.Status == RequestStatus.Success)
        {
            return Ok(_mapper.ToGetWalletsApiResponse(result));
        }
        
        return BadRequest("Some error has been occured.");
    }
    
    [HttpGet("{spaceId}")]
    public async Task<IActionResult> GetWallet([FromRoute] Guid spaceId, [FromQuery] Guid walletId)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");

        var result = await _walletsApi.GetWallet(new(spaceId, walletId));

        if (result.Status == RequestStatus.Success)
        {
            return Ok(_mapper.ToWalletApiResponse(result.Wallet!));
        }
        
        return BadRequest("Some error has been occured.");
    }
    
    [HttpPost("create/{spaceId}")]
    public async Task<IActionResult> CreateWallet([FromRoute] Guid spaceId, [FromBody] CreateWalletApiRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);

        if (!valid) return BadRequest("User doesn't have provided space.");
        
        var apiRequest = _mapper.ToApiCreateWalletRequest(request, spaceId);
        var result = await _walletsApi.CreateWallet(apiRequest);

        if (result.Status == RequestStatus.Success)
        {
            return Ok(_mapper.ToCreateWalletResponse(result, apiRequest));
        }

        return BadRequest("Some error has been occured.");
    }

    [HttpPost("edit/{spaceId}")]
    public async Task<IActionResult> EditWallet([FromRoute] Guid spaceId, [FromBody] EditWalletApiRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");
        
        var apiRequest = _mapper.ToApiEditWalletRequest(request, spaceId);
        var result = await _walletsApi.EditWallet(apiRequest);
        
        if (result.Status == RequestStatus.Success)
        {
            return Ok();
        }

        return BadRequest("Some error has been occured.");
    }
    
    [HttpPost("archive/{spaceId}")]
    public async Task<IActionResult> ArchiveWallet([FromRoute] Guid spaceId, [FromBody] ArchiveWalletApiRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");
        
        var apiRequest = new Wallets.Contracts.Contracts.ArchiveWalletRequest(request.WalletId, spaceId);
        var result = await _walletsApi.ArchiveWallet(apiRequest);
        
        if (result.Status == RequestStatus.Success)
        {
            return Ok();
        }

        return BadRequest("Some error has been occured.");
    }
}