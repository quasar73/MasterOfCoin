using Categories.Contracts.Contracts;
using Categories.Contracts.Contracts.Enums;
using Categories.Contracts.Interfaces;
using MasterOfCoin.API.ApiContracts.Category;
using MasterOfCoin.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MasterOfCoin.API.Controllers;

public class CategoryController(
    ICategoriesApi _categoriesApi, 
    IContractMapper _mapper, 
    IValidationService _validationService) : Controller
{
    [HttpPost("{spaceId}")]
    public async Task<IActionResult> GetCategory([FromRoute] Guid spaceId, [FromQuery] Guid categoryId)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");

        var apiRequest = new GetCategoryRequest(categoryId, spaceId);
        var result = await _categoriesApi.GetCategory(apiRequest);

        if (result.Status == RequestStatus.Success)
        {
            return Ok(_mapper.ToCategoryApiResponse(result.Category!));
        }
        
        return BadRequest("Some error has been occured.");
    }
    
    [HttpPost("get-list/{spaceId}")]
    public async Task<IActionResult> GetCategories([FromRoute] Guid spaceId)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");

        var apiRequest = new GetCategoriesRequest(spaceId);
        var result = await _categoriesApi.GetCategories(apiRequest);

        if (result.Status == RequestStatus.Success)
        {
            return Ok(_mapper.ToGetCategoriesApiResponse(result.Categories));
        }
        
        return BadRequest("Some error has been occured.");
    }
    
    [HttpPost("create/{spaceId}")]
    public async Task<IActionResult> CreateCategory([FromRoute] Guid spaceId, [FromBody] CreateCategoryApiRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");

        var apiRequest = _mapper.ToCreateCategoryRequest(request, spaceId);
        var result = await _categoriesApi.CreateCategory(apiRequest);

        if (result.Status == RequestStatus.Success)
        {
            return Ok(_mapper.ToCreateCategoryApiResponse(result, apiRequest));
        }
        
        return BadRequest("Some error has been occured.");
    }
    
    [HttpPost("edit/{spaceId}")]
    public async Task<IActionResult> EditCategory([FromRoute] Guid spaceId, [FromBody] EditCategoryApiRequest request)
    {
        var username = HttpContext.User.Identity!.Name!;
        var valid = await _validationService.ValidateUserSpace(username, spaceId);
        
        if (!valid) return BadRequest("User doesn't have provided space.");

        var apiRequest = _mapper.ToEditCategoryRequest(request, spaceId);
        var result = await _categoriesApi.EditCategory(apiRequest);

        if (result.Status == RequestStatus.Success)
        {
            return Ok();
        }
        
        return BadRequest("Some error has been occured.");
    }
}