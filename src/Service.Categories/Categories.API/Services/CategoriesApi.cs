using Categories.API.Data.Interfaces;
using Categories.API.Services.Interfaces;
using Categories.Contracts.Contracts;
using Categories.Contracts.Contracts.Enums;
using Categories.Contracts.Interfaces;
using Lib.MessageBroker.Contracts;

namespace Categories.API.Services;

public class CategoriesApi(ICategoryRepository _repository, IModelMapper _mapper, IPublisher _publisher) : ICategoriesApi
{
    public async Task<CreateCategoryResponse> CreateCategory(CreateCategoryRequest request)
    {
        var categoryInDb = _mapper.ToCategoryInDb(request);

        if (request.ParentId.HasValue)
        {
            var parentCategory = await _repository.Find(request.ParentId.Value, request.SpaceId);
            if (parentCategory is null) return new(RequestStatus.NotFound, default);
            if (parentCategory.ParentId is not null) return new(RequestStatus.InvalidParent, default);
        }

        var affectedRows = await _repository.Create(categoryInDb);

        if (affectedRows != 1) return new(RequestStatus.InvalidData, default);

        await _publisher.Publish(_mapper.ToCreateAccountMessage(categoryInDb));

        return new(RequestStatus.Success, categoryInDb.Id);
    }

    public async Task<StatusResponse> EditCategory(EditCategoryRequest request)
    {
        var categoryInDb = await _repository.Find(request.Id, request.SpaceId);

        if (categoryInDb is null) return new(RequestStatus.NotFound);
        
        if (request.ParentId.HasValue)
        {
            var parentCategory = await _repository.Find(request.ParentId.Value, request.SpaceId);
            if (parentCategory is null) return new(RequestStatus.NotFound);
            if (parentCategory.ParentId is not null || parentCategory.ParentId == request.Id) return new(RequestStatus.InvalidParent);
        }

        categoryInDb.Name = request.Name ?? categoryInDb.Name;
        categoryInDb.ParentId = request.ParentId;
        categoryInDb.Color = request.Color;
        categoryInDb.Icon = request.Icon;

        var affectedRows = await _repository.Update(categoryInDb);

        if (affectedRows != 1) return new(RequestStatus.InvalidData);

        return new(RequestStatus.Success);
    }

    public async Task<CategoriesListResponse> GetCategories(GetCategoriesRequest request)
    {
        var categoriesInDb = await _repository.GetList(request.SpaceId);

        return new(RequestStatus.Success, _mapper.ToCategoryResponses(categoriesInDb));
    }

    public async Task<GetCategoryResponse> GetCategory(GetCategoryRequest request)
    {
        var categoryInDb = await _repository.Find(request.Id, request.SpaceId);

        if (categoryInDb is null) return new(RequestStatus.NotFound, default);

        return new(RequestStatus.Success, _mapper.ToCategoryResponse(categoryInDb));
    }
}