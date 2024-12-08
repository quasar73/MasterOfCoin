using Transactions.Contracts.Messages.Accounts.Enums;

namespace Transactions.Contracts.Messages.Accounts;

public record CreateAccountMessage(Guid Id, Guid SpaceId, AccountCreatingSource Source);