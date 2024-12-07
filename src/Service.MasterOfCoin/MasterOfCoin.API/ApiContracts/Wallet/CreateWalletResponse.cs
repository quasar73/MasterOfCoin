﻿namespace MasterOfCoin.API.ApiContracts.Wallet;

public record CreateWalletResponse(
    Guid Id,
    string Name, 
    decimal InitialValue, 
    string Currency, 
    Guid SpaceId, 
    bool Cumulative);