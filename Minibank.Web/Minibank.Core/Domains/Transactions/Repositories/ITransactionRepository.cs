using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Transactions.Repositories
{
    public interface ITransactionRepository
    {
        Task<string> CreateAsync(Transaction transaction, CancellationToken cancellationToken);
        Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken);
    }
}
