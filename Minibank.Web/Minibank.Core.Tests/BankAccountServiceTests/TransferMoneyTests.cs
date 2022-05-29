using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.BankAccounts.Services;
using Minibank.Core.Domains.Currencies;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions;
using Minibank.Core.Domains.Transactions.Repositories;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Tests.Utils;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.BankAccountServiceTests
{
    public class TransferMoneyTests
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly Mock<ITransactionService> _transactionService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBankAccountRepository> _bankAccountRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IUnitOfWork> _saveChangesMock;
        private readonly Mock<ICurrencyConverter> _currencyConverterMock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public TransferMoneyTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _bankAccountRepositoryMock = new Mock<IBankAccountRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _saveChangesMock = new Mock<IUnitOfWork>();
            _currencyConverterMock = new Mock<ICurrencyConverter>();
            _transactionService = new Mock<ITransactionService>();

            _bankAccountService = new BankAccountService(_bankAccountRepositoryMock.Object, _transactionService.Object,
                _currencyConverterMock.Object, _userRepositoryMock.Object, _saveChangesMock.Object);

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        [Fact]
        public async Task TransferMoney_FromAccountDontExist_ShouldThrowException()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 1;
            BankAccount fromBankAccount = null;
            var toBankAccount = new BankAccount(toBankAccountId, AvaliableCurrencies.RUB, 0) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>
                (() => _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(BankAccount)} с id = {fromBankAccountId} не найден", exception.Message);
        }

        [Fact]
        public async Task TransferMoney_ToAccountDontExist_ShouldThrowException()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 1;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var fromBankAccount = new BankAccount(fromBankAccountId, AvaliableCurrencies.RUB, fromAccountBalance) { IsActive = true };
            BankAccount toBankAccount = null;

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>
                (() => _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(BankAccount)} с id = {toBankAccountId} не найден", exception.Message);
        }

        [Theory]
        [InlineData(true, false, "toAccountId")]
        [InlineData(false, true, "fromAccountId")]
        public async Task CalculateComission_OneOfAccountsAlreadyClosed_ShouldThrowException(bool fromAccountState, bool toAccountState, string closedAccId)
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 1;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            decimal toAccountBalance = 0;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = fromAccountState };
            var toBankAccount = new BankAccount(toBankAccountId, currency, toAccountBalance) { IsActive = toAccountState };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains($"Банковский аккаунт с id = {closedAccId} уже закрыт", exception.Message);
        }

        [Fact]
        public async Task TransferMoney_NonPositiveTransfer_ShouldThrowException()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 0;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains("Введите сумму больше 0", exception.Message);
        }

        [Fact]
        public async Task TransferMoney_NotEnoughFromAccountBalance_ShouldThrowException()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            decimal fromAccountBalance = amountToBeTransferred - 1;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains("Недостаточно средств на счете", exception.Message);
        }

        [Fact]
        public async Task TransferMoney_SameAccounts_ShouldThrowException()
        {
            //ARRANGE
            string fromBankAccountId = "AccountId";
            string toBankAccountId = "AccountId";
            decimal amountToBeTransferred = 5;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains("Выберите разные аккаунты", exception.Message);
        }

        [Fact]
        public async Task TransferMoney_SuccessPass_SaveChangesOnce()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            _saveChangesMock.Verify(saver => saver.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task TransferMoney_SuccessPass_ReturnResult(int numberOfDbChanges, bool expectedResult)
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(numberOfDbChanges);

            //ACT
            var result = await _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransferMoney_SuccessPass_CreatesTransactionOnce()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var fromBankAccount = new BankAccount(fromBankAccountId, AvaliableCurrencies.RUB, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, AvaliableCurrencies.RUB, fromAccountBalance) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            _transactionService.Verify(saver => saver.CreateAsync(It.IsAny<Transaction>(), _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task TransferMoney_SuccessPass_CreatesTransactionWithRightModel()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            var currency = AvaliableCurrencies.RUB;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };
            Transaction referenceTransaction = new Transaction(amountToBeTransferred, currency, fromBankAccountId, toBankAccountId);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            _transactionService.Verify(transactionService => transactionService.
            CreateAsync(It.Is<Transaction>(x => MyMockExtensions.PublicInstancePropertiesEqual(x, referenceTransaction) == true), _cancellationToken));
        }

        [Fact]
        public async Task TransferMoney_SuccessPass_UpdateBalanceTwice()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            var currency = AvaliableCurrencies.RUB;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };
            Transaction referenceTransaction = new Transaction(amountToBeTransferred, currency, fromBankAccountId, toBankAccountId);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            _bankAccountRepositoryMock.Verify(repo => repo.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>(), _cancellationToken), Times.Exactly(2));
        }

        [Fact]
        public async Task TransferMoney_SuccessPass_UpdateBalanceWithRightParameters()
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal amountToBeTransferred = 5;
            var currency = AvaliableCurrencies.RUB;
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { IsActive = true };
            Transaction referenceTransaction = new Transaction(amountToBeTransferred, currency, fromBankAccountId, toBankAccountId);
            
            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);            
            
            _currencyConverterMock.Setup(converter => converter.ConvertCurrencyAsync(It.IsAny<decimal>(), It.IsAny<AvaliableCurrencies>(), 
                It.IsAny<AvaliableCurrencies>(), _cancellationToken))
                .ReturnsAsync(amountToBeTransferred);

            //ACT
            var result = await _bankAccountService.TransferMoneyAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            _bankAccountRepositoryMock.Verify(repo => repo
            .UpdateBalanceAsync(It.Is<string>(x => x == fromBankAccountId), It.Is<decimal>(x => x == -amountToBeTransferred), _cancellationToken));
            _bankAccountRepositoryMock.Verify(repo => repo
            .UpdateBalanceAsync(It.Is<string>(x => x == toBankAccountId), It.Is<decimal>(x => x == amountToBeTransferred), _cancellationToken));
        }
    }
}
