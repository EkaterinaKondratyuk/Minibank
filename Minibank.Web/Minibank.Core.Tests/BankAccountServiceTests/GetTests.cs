using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.BankAccounts.Services;
using Minibank.Core.Domains.Currencies;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions.Repositories;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.BankAccountServiceTests
{
    public class GetTests
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly ITransactionService _transactionService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBankAccountRepository> _bankAccountRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IUnitOfWork> _saveChangesMock;
        private readonly Mock<ICurrencyConverter> _curencyConverterMock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public GetTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _bankAccountRepositoryMock = new Mock<IBankAccountRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _saveChangesMock = new Mock<IUnitOfWork>();
            _curencyConverterMock = new Mock<ICurrencyConverter>();

            _transactionService = new TransactionService(_transactionRepositoryMock.Object);
            _bankAccountService = new BankAccountService(_bankAccountRepositoryMock.Object, _transactionService,
                _curencyConverterMock.Object, _userRepositoryMock.Object, _saveChangesMock.Object);

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        [Fact]
        public async Task GetAllBankAccounts_SuccessPass_ReturnList()
        {
            //ARRANGE
            var currency = AvaliableCurrencies.RUB;
            var listOfAccounts = new List<BankAccount>();

            for (int i = 0; i < 5; i++)
            {
                listOfAccounts.Add(new BankAccount(i.ToString(), currency, 0));
            }

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetAllAsync(_cancellationToken))
                .ReturnsAsync(listOfAccounts);

            //ACT
            var result = await _bankAccountService.GetAll(_cancellationToken);

            //ASSERT
            Assert.Equal(listOfAccounts, result);
        }

        [Fact]
        public async Task GetById_DontExist_ShouldThrowException()
        {
            //ARRANGE
            var id = "id123";
            BankAccount bankAccount = null;

            _bankAccountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(bankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>
                (() => _bankAccountService.GetByIdAsync(id, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(BankAccount)} с id = {id} не найден", exception.Message);
        }

        [Fact]
        public async Task GetById_SuccessPass_CallRepositoryGetByIdWithRightId()
        {
            //ARRANGE
            var id = "id123";
            var expectedBankAccount = new BankAccount("userId", AvaliableCurrencies.USD, 10)
            { Id = id, IsActive = false, ClosingDate = DateTime.UtcNow, OpeningDate = DateTime.UtcNow };

            _bankAccountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            //ACT
            var result = await _bankAccountService.GetByIdAsync(id, _cancellationToken);

            //ASSERT
            _bankAccountRepositoryMock.Verify(repo => repo
            .GetByIdAsync(It.Is<string>(x => x == id), _cancellationToken));
        }
        [Fact]
        public async Task GetById_SuccessPass_ReturnBankAccount()
        {
            //ARRANGE
            var id = "id123";
            var expectedBankAccount = new BankAccount("userId", AvaliableCurrencies.USD, 10)
            { Id = id, IsActive = false, ClosingDate = DateTime.UtcNow, OpeningDate = DateTime.UtcNow };

            _bankAccountRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            //ACT
            var result = await _bankAccountService.GetByIdAsync(id, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedBankAccount, result);
        }
    }
}
