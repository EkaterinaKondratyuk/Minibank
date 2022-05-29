using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.UserRepository;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.BankAccounts.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ITransactionService _transactionService;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BankAccountService(IBankAccountRepository bankAccountRepository, ITransactionService transactionService,
            ICurrencyConverter currencyConverter, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _bankAccountRepository = bankAccountRepository;
            _transactionService = transactionService;
            _currencyConverter = currencyConverter;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> CalculateComissionAsync(decimal amount, string fromAccountId, string toAccountId, CancellationToken cancellationToken)
        {
            var fromAccount = await _bankAccountRepository.GetByIdAsync(fromAccountId, cancellationToken);
            var toAccount = await _bankAccountRepository.GetByIdAsync(toAccountId, cancellationToken);

            ValidateTransferAndThrow(amount, fromAccountId, fromAccount, toAccountId, toAccount);

            if (fromAccount.UserId == toAccount.UserId)
                return 0;

            decimal comission = amount * 0.02m;
            return decimal.Round(comission, 2);
        }

        public async Task<bool> CloseAsync(string accountId, CancellationToken cancellationToken)
        {
            await ValidateAccountClosureAndThrowAsync(accountId, cancellationToken);
            await _bankAccountRepository.CloseAsync(accountId, cancellationToken);
            int numberOfDbChanges = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return numberOfDbChanges > 0;
        }

        public async Task<string> CreateAsync(BankAccount bankAccount, CancellationToken cancellationToken)
        {
            await ValidateAccountCreationAndThrowAsync(bankAccount.UserId, bankAccount.Balance, cancellationToken);

            string bankAccountId = await _bankAccountRepository.CreateAsync(bankAccount, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return bankAccountId;
        }

        public Task<List<BankAccount>> GetAll(CancellationToken cancellationToken)
        {
            return _bankAccountRepository.GetAllAsync(cancellationToken);
        }

        public async Task<BankAccount> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(id, cancellationToken);
            if (bankAccount == null)
                throw new ObjectNotFoundException($"Объект {nameof(BankAccount)} с id = {id} не найден");
            return bankAccount;
        }

        public async Task<bool> TransferMoneyAsync(decimal amount, string fromAccountId, string toAccountId, CancellationToken cancellationToken)
        {
            var fromAccount = await _bankAccountRepository.GetByIdAsync(fromAccountId, cancellationToken);
            var toAccount = await _bankAccountRepository.GetByIdAsync(toAccountId, cancellationToken);

            ValidateTransferAndThrow(amount, fromAccountId, fromAccount, toAccountId, toAccount);

            decimal comission = await CalculateComissionAsync(amount, fromAccountId, toAccountId, cancellationToken);

            decimal transferedAmount = await _currencyConverter.ConvertCurrencyAsync(amount - comission, fromAccount.Currency, toAccount.Currency, cancellationToken);

            await _bankAccountRepository.UpdateBalanceAsync(fromAccountId, -amount, cancellationToken);
            await _bankAccountRepository.UpdateBalanceAsync(toAccountId, transferedAmount, cancellationToken);

            Transaction transaction = new Transaction(amount, fromAccount.Currency, fromAccountId, toAccountId);

            await _transactionService.CreateAsync(transaction, cancellationToken);
            int numberOfDbChanges = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return numberOfDbChanges > 0;
        }

        private async Task ValidateAccountCreationAndThrowAsync(string userId, decimal startAmount, CancellationToken cancellationToken)
        {
            if (await _userRepository.ExistsAsync(userId, cancellationToken) == false)
                throw new ObjectNotFoundException($"Объект {nameof(User)} с id = {userId} не найден");

            if (startAmount < 0)
                throw new ValidationException("Введите неотрицательное значение");
        }

        private void ValidateTransferAndThrow(decimal amount, string fromAccountId, BankAccount fromAccount, string toAccountId, BankAccount toAccount)
        {
            if (fromAccount == null)
                throw new ObjectNotFoundException($"Объект {nameof(BankAccount)} с id = {fromAccountId} не найден");
            if (fromAccount.IsActive == false)
                throw new ValidationException($"Банковский аккаунт с id = {fromAccountId} уже закрыт");
            if (toAccount == null)
                throw new ObjectNotFoundException($"Объект {nameof(BankAccount)} с id = {toAccountId} не найден");
            if (toAccount.IsActive == false)
                throw new ValidationException($"Банковский аккаунт с id = {toAccountId} уже закрыт");
            if (amount <= 0)
                throw new ValidationException("Введите сумму больше 0");
            if (amount > fromAccount.Balance)
                throw new ValidationException("Недостаточно средств на счете");
            if (fromAccountId == toAccountId)
                throw new ValidationException("Выберите разные аккаунты");
        }

        private async Task ValidateAccountClosureAndThrowAsync(string id, CancellationToken cancellationToken)
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(id, cancellationToken);
            if (bankAccount == null)
                throw new ObjectNotFoundException($"Объект {nameof(BankAccount)} с id = {id} не найден");
            if (bankAccount.IsActive == false)
                throw new ValidationException($"Банковский аккаунт с id = {id} уже закрыт");
            if (bankAccount.Balance != 0)
                throw new ValidationException("Аккаунт с ненулевым счётом не может быть закрыт");
        }
    }
}
