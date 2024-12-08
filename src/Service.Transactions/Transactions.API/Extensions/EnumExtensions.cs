using Transactions.API.Data.Models.Enums;
using Transactions.Contracts.Messages.Accounts.Enums;

namespace Transactions.API.Extensions;

public static class EnumExtensions
{
    public static AccountType ToAccountType(this AccountCreatingSource source) => source switch
    {
        AccountCreatingSource.Wallet => AccountType.Wallet,
        AccountCreatingSource.Category => AccountType.Category,
        _ => AccountType.None
    };
}