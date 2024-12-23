﻿using Wallets.Contracts.Contracts.Enums;

namespace Wallets.Contracts.Contracts;

public record CreateWalletResponse(RequestStatus Status, Guid? WalletId);