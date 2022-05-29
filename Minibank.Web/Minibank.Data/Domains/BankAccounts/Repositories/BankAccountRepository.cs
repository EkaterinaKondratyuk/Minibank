using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Minibank.Core;
using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.BankAccounts.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Data.Domains.BankAccounts.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly MinibankContext _context;
        private readonly IMapper _mapper;

        public BankAccountRepository(MinibankContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<BankAccount>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<BankAccountDbModel> bankAccounts = await _context.BankAccounts.AsNoTracking().ToListAsync(cancellationToken);
            return _mapper.Map<List<BankAccountDbModel>, List <BankAccount>>(bankAccounts);
        }

        public async Task<string> CreateAsync(BankAccount bankAccount, CancellationToken cancellationToken)
        {
            bankAccount.Id = Guid.NewGuid().ToString();
            bankAccount.OpeningDate = DateTime.UtcNow;
            bankAccount.IsActive = true;

            BankAccountDbModel entity = _mapper.Map<BankAccount, BankAccountDbModel>(bankAccount);

            await _context.BankAccounts.AddAsync(entity, cancellationToken);
            return entity.Id;
        }

        public async Task UpdateBalanceAsync(string id, decimal transferredAmount, CancellationToken cancellationToken)
        {
            var bankAccount = await GetDbBankAccountByIdAsync(id, cancellationToken);
            if (bankAccount == null)
                throw new ObjectNotFoundException($"Объект {nameof(bankAccount.GetType)} с id = {id} не найден");

            bankAccount.Balance += transferredAmount;
            _context.BankAccounts.Update(bankAccount);
        }

        public async Task CloseAsync(string id, CancellationToken cancellationToken)
        {
            var bankAccount = await GetDbBankAccountByIdAsync(id, cancellationToken);
            if (bankAccount == null)
                throw new ObjectNotFoundException($"Объект {nameof(bankAccount.GetType)} с id = {id} не найден");

            bankAccount.IsActive = false;
            bankAccount.ClosingDate = DateTime.UtcNow;
            _context.BankAccounts.Update(bankAccount);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _context.BankAccounts.AsNoTracking().AnyAsync(x => x.UserId == userId, cancellationToken);
        }

        public async Task<BankAccount> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            BankAccountDbModel bankAccountDb = await GetDbBankAccountByIdAsync(id, cancellationToken);
            if (bankAccountDb == null)
                return null;

            return _mapper.Map<BankAccountDbModel, BankAccount>(bankAccountDb);
        }

        private async Task<BankAccountDbModel> GetDbBankAccountByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
