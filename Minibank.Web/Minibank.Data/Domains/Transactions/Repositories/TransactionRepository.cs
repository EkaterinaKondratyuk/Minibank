using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Minibank.Core.Domains.Transactions;
using Minibank.Core.Domains.Transactions.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Data.Domain.Transactions.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly MinibankContext _context;
        private readonly IMapper _mapper;

        public TransactionRepository(MinibankContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<string> CreateAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            transaction.Id = Guid.NewGuid().ToString();
            transaction.CreatedAt = DateTime.UtcNow;
            TransactionDbModel entity = _mapper.Map<Transaction, TransactionDbModel>(transaction);

            await _context.Transactions.AddAsync(entity, cancellationToken);
            return entity.Id;
        }

        public async Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<TransactionDbModel> transactions = await _context.Transactions.AsNoTracking().ToListAsync(cancellationToken);
            return _mapper.Map<List<TransactionDbModel>, List <Transaction>>(transactions);
        }
    }
}
