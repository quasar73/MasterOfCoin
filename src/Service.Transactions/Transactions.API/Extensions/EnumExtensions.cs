using Transactions.API.Data.Models.Enums;
using Transactions.Contracts.Messages.Accounts;

namespace Transactions.API.Extensions;

public static class EnumExtensions
{
    public static AccountType ToAccountType(this AccountCreatingSource source) => source switch
    {
        AccountCreatingSource.Wallet => AccountType.Wallet,
        _ => AccountType.None
    };
}