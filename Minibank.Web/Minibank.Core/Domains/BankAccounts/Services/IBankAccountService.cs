using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.BankAccounts.Services
{
    public interface IBankAccountService
    {
        Task<string> CreateAsync(BankAccount bankAccount, CancellationToken cancellationToken);
        Task<List<BankAccount>> GetAll(CancellationToken cancellationToken);
        Task<BankAccount> GetByIdAsync(string id, CancellationToken cancellationToken);
        Task<bool> CloseAsync(string accountId, CancellationToken cancellationToken);
        Task<decimal> CalculateComissionAsync(decimal amount, string fromAccountId, string toAccountId, CancellationToken cancellationToken);
        Task<bool> TransferMoneyAsync(decimal amount, string fromAccountId, string toAccountId, CancellationToken cancellationToken);
    }
}
