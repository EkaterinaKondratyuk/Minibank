using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.BankAccounts.Services;
using Minibank.Core.Domains.Currencies;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions.Repositories;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.BankAccountServiceTests
{
    public class CalculateComissionTests
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

        public CalculateComissionTests()
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
        public async Task CalculateComission_FromAccountDontExist_ShouldThrowException()
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
                (() => _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(BankAccount)} с id = {fromBankAccountId} не найден", exception.Message);
        }

        [Fact]
        public async Task CalculateComission_ToAccountDontExist_ShouldThrowException()
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
                (() => _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

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
        public async Task CalculateComission_NonPositiveTransfer_ShouldThrowException()
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
                (() => _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains("Введите сумму больше 0", exception.Message);
        }

        [Fact]
        public async Task CalculateComission_NotEnoughFromAccountBalance_ShouldThrowException()
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
                (() => _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains("Недостаточно средств на счете", exception.Message);
        }

        [Fact]
        public async Task CalculateComission_SameAccounts_ShouldThrowException()
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
                (() => _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken));

            //ASSERT
            Assert.Contains("Выберите разные аккаунты", exception.Message);
        }

        [Theory]
        [InlineData("userId", "userId", 10, 0)]
        [InlineData("fromUserId", "toUserId", 10, 0.2)]
        public async Task CalculateComission_SuccessPass_ReturnRightComission(string fromUserId, string toUserId, decimal amountToBeTransferred, decimal expectedResult)
        {
            //ARRANGE
            string fromBankAccountId = "fromAccountId";
            string toBankAccountId = "toAccountId";
            decimal fromAccountBalance = amountToBeTransferred + 1;
            var currency = AvaliableCurrencies.RUB;
            var fromBankAccount = new BankAccount(fromBankAccountId, currency, fromAccountBalance) { UserId = fromUserId, IsActive = true };
            var toBankAccount = new BankAccount(toBankAccountId, currency, fromAccountBalance) { UserId = toUserId, IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(fromBankAccountId, _cancellationToken))
                .ReturnsAsync(fromBankAccount);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(toBankAccountId, _cancellationToken))
                .ReturnsAsync(toBankAccount);

            //ACT
            var result = await _bankAccountService.CalculateComissionAsync(amountToBeTransferred, fromBankAccountId, toBankAccountId, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedResult, result);
        }
    }
}
