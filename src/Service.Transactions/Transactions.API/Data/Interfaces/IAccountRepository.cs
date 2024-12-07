using Transactions.API.Data.Models;

namespace Transactions.API.Data.Interfaces;

public interface IAccountRepository
{
    Task CreateAccount(AccountInDb accountInDb);
}