using MasterOfCoin.API.ApiContracts.Space;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SpaceController(ISpaceService _service, IContractMapper _mapper) : Controller
{
    [HttpGet("get-list")]
    public async Task<IActionResult> GetUserSpaces()
    {
        var username = HttpContext.User.Identity!.Name!;
        
        var result = await _service.GetList(username);
        return Ok(_mapper.ToSpaceResponses(result));
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        
        var spaceInDb = await _service.CreateSpace(request.Name, username);
        return Ok(_mapper.ToSpaceResponse(spaceInDb));
    }
    
    [HttpPost("delete")]
    public async Task<IActionResult> DeleteSpace([FromBody] DeleteSpaceRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        
        await _service.DeleteSpace(request.SpaceId, username);
        return Ok();
    }
}