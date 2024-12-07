namespace Transactions.Contracts.Messages.Accounts;

public record CreateAccountMessage(Guid Id, string Name, Guid SpaceId);