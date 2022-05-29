using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Transactions.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly Repositories.ITransactionRepository _transactionRepository;

        public TransactionService(Repositories.ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<string> CreateAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            return await _transactionRepository.CreateAsync(transaction, cancellationToken);
        }

        public Task<List<Transaction>> GetAll(CancellationToken cancellationToken)
        {
            return _transactionRepository.GetAllAsync(cancellationToken);
        }
    }
}
