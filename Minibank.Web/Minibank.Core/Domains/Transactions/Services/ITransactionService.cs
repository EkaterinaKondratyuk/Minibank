using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Transactions.Services
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetAll(CancellationToken cancellationToken);
        Task<string> CreateAsync(Transaction transaction, CancellationToken cancellationToken);
    }
}
