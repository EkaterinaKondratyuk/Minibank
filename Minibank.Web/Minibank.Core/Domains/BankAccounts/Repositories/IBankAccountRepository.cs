using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.BankAccounts.Repositories
{
    public interface IBankAccountRepository
    {
        Task<List<BankAccount>> GetAllAsync(CancellationToken cancellationToken);
        Task<string> CreateAsync(BankAccount bankAccount, CancellationToken cancellationToken);
        Task UpdateBalanceAsync(string id, decimal amount, CancellationToken cancellationToken);
        Task CloseAsync(string id, CancellationToken cancellationToken);
        Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task<BankAccount> GetByIdAsync(string id, CancellationToken cancellationToken);
    }
}
