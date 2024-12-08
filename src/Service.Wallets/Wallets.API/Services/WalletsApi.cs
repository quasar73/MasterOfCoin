using Lib.MessageBroker.Contracts;
using Wallets.API.Data.Interfaces;
using Wallets.API.Data.Models;
using Wallets.API.Services.Interfaces;
using Wallets.Contracts.Contracts;
using Wallets.Contracts.Contracts.Enums;
using Wallets.Contracts.Interfaces;

namespace Wallets.API.Services;

public class WalletsApi(
    IWalletRepository _repository, 
    IPublisher _publisher, 
    IModelMapper _mapper) : IWalletsApi
{
    public async Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request)
    {
        var walletInDb = new WalletInDb
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Currency = request.Currency,
            SpaceId = request.SpaceId,
            Value = request.InitialValue,
            Cumulative = request.Cumulative,
            AccountId = Guid.NewGuid(),
            Archived = false
        };
        var affectedRows = await _repository.Create(walletInDb);

        if (affectedRows != 1) return new(RequestStatus.InvalidData, default);
        
        await _publisher.Publish(_mapper.ToCreateAccountMessage(walletInDb));

        return new(RequestStatus.Success, walletInDb.Id);
    }

    public async Task<StatusResponse> EditWallet(EditWalletRequest request)
    {
        var walletInDb = await _repository.Find(request.WalletId, request.SpaceId);
        
        if (walletInDb is null) return new(RequestStatus.NotFound);
        
        walletInDb.Value = request.Value ?? walletInDb.Value;
        walletInDb.Name = request.Name ?? walletInDb.Name;
        walletInDb.Currency = request.Currency ?? walletInDb.Currency;
        walletInDb.Cumulative = request.Cumulative ?? walletInDb.Cumulative;
        
        var affectedRows = await _repository.Update(walletInDb);

        if (affectedRows != 1) return new(RequestStatus.InvalidData);
        
        return new(RequestStatus.Success);
    }

    public async Task<StatusResponse> ArchiveWallet(ArchiveWalletRequest request)
    {
        var walletInDb = await _repository.Find(request.WalletId, request.SpaceId);

        if (walletInDb is null) return new(RequestStatus.NotFound);
        
        walletInDb.Archived = true;
        
        var affectedRows = await _repository.Update(walletInDb);

        if (affectedRows != 1) return new(RequestStatus.InvalidData);
        
        return new(RequestStatus.Success);
    }

    public async Task<WalletsListResponse> GetWallets(GetWalletsRequest request)
    {
        var wallets = await _repository.GetList(request.SpaceId);

        return new(RequestStatus.Success, wallets.Select(_mapper.ToWalletResponse).ToArray());
    }

    public async Task<GetWalletResponse> GetWallet(GetWalletRequest request)
    {
        var walletInDb = await _repository.Find(request.WalletId, request.SpaceId);
        
        if (walletInDb is null) return new(RequestStatus.NotFound, default);
        
        return new(RequestStatus.Success, _mapper.ToWalletResponse(walletInDb));
    }
}