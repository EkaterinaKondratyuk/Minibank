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
    public class CloseTests
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

        public CloseTests()
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
        public async Task CloseBankAccount_DontExist_ShouldThrowException()
        {
            //ARRANGE
            var id = "id123";
            BankAccount expectedBankAccount = null;

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>(() => _bankAccountService.CloseAsync(id, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(BankAccount)} с id = {id} не найден", exception.Message);
        }

        [Fact]
        public async Task CloseBankAccount_AlreadyClosed_ShouldThrowException()
        {
            //ARRANGE
            string accountId = "accountId";
            var expectedBankAccount = new BankAccount("userId", AvaliableCurrencies.USD, 0) { IsActive = false };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.CloseAsync(accountId, _cancellationToken));

            //ASSERT
            Assert.Contains($"Банковский аккаунт с id = {accountId} уже закрыт", exception.Message);
        }

        [Fact]
        public async Task CloseBankAccount_NonzeroBalance_ShouldThrowException()
        {
            //ARRANGE
            var expectedBankAccount = new BankAccount("userId", AvaliableCurrencies.USD, 1) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.CloseAsync(It.IsAny<string>(), _cancellationToken));

            //ASSERT
            Assert.Contains("Аккаунт с ненулевым счётом не может быть закрыт", exception.Message);
        }

        [Fact]
        public async Task CloseBankAccount_SuccessPass_SaveChangesOnce()
        {
            //ARRANGE
            var expectedBankAccount = new BankAccount("userId", AvaliableCurrencies.USD, 0) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _bankAccountService.CloseAsync(It.IsAny<string>(), _cancellationToken);

            //ASSERT
            _saveChangesMock.Verify(saver => saver.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task CloseBankAccount_SuccessPass_ReturnResult(int numberOfDbChanges, bool expectedResult)
        {
            //ARRANGE
            var expectedBankAccount = new BankAccount("userId", AvaliableCurrencies.USD, 0) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(numberOfDbChanges);

            //ACT
            var result = await _bankAccountService.CloseAsync(It.IsAny<string>(), _cancellationToken);

            //ASSERT
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task CloseBankAccount_SuccessPass_CallRepositoryUpdateWithRightId()
        {
            //ARRANGE
            string bankAccountId = "userId";
            var expectedBankAccount = new BankAccount(bankAccountId, AvaliableCurrencies.USD, 0) { IsActive = true };

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(expectedBankAccount);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _bankAccountService.CloseAsync(bankAccountId, _cancellationToken);

            //ASSERT
            _bankAccountRepositoryMock.Verify(bankRepository => bankRepository.CloseAsync(It.Is<string>(x => x == bankAccountId), _cancellationToken));
        }
    }
}
