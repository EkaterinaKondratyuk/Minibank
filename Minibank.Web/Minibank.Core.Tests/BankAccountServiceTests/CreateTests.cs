using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.BankAccounts.Services;
using Minibank.Core.Domains.Currencies;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions.Repositories;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Tests.Utils;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.BankAccountServiceTests
{
    public class CreateTests
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

        public CreateTests()
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
        public async Task CreateBankAccount_OwnerUserDontExist_ShouldThrowException()
        {
            //ARRANGE
            var userId = "userId";
            decimal startAmount = 0;
            AvaliableCurrencies currency = AvaliableCurrencies.RUB;
            BankAccount bankAccount = new BankAccount(userId, currency, startAmount);
            string expectedAccountId = "accountId";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.CreateAsync(It.IsAny<BankAccount>(), _cancellationToken))
                .ReturnsAsync(expectedAccountId);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>
                (() => _bankAccountService.CreateAsync(bankAccount, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(User)} с id = {userId} не найден", exception.Message);
        }

        [Fact]
        public async Task CreateBankAccount_NegativeStartBalance_ShouldThrowException()
        {
            //ARRANGE
            var userId = "userId";
            decimal startAmount = -1;
            AvaliableCurrencies currency = AvaliableCurrencies.RUB;
            BankAccount bankAccount = new BankAccount(userId, currency, startAmount);
            string expectedAccountId = "accountId";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.CreateAsync(It.IsAny<BankAccount>(), _cancellationToken))
                .ReturnsAsync(expectedAccountId);

            _saveChangesMock
                .Setup(fakeSaveChanges => fakeSaveChanges.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>
                (() => _bankAccountService.CreateAsync(bankAccount, _cancellationToken));

            //ASSERT
            Assert.Contains("Введите неотрицательное значение", exception.Message);
        }

        [Fact]
        public async Task CreateBankAccount_SuccessPass_SaveChangesOnce()
        {
            //ARRANGE
            var userId = "userId";
            decimal startAmount = 0;
            AvaliableCurrencies currency = AvaliableCurrencies.RUB;
            BankAccount bankAccount = new BankAccount(userId, currency, startAmount);
            string expectedAccountId = "accountId";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.CreateAsync(It.IsAny<BankAccount>(), _cancellationToken))
                .ReturnsAsync(expectedAccountId);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var accountId = await _bankAccountService.CreateAsync(bankAccount, _cancellationToken);

            //ASSERT
            _saveChangesMock.Verify(saver => saver.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Fact]
        public async Task CreateBankAccount_SuccessPass_ReturnAccountId()
        {
            //ARRANGE
            var userId = "userId";
            decimal startAmount = 0;
            AvaliableCurrencies currency = AvaliableCurrencies.RUB;
            BankAccount bankAccount = new BankAccount(userId, currency, startAmount);
            string expectedAccountId = "accountId";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.CreateAsync(It.IsAny<BankAccount>(), _cancellationToken))
                .ReturnsAsync(expectedAccountId);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var accountId = await _bankAccountService.CreateAsync(bankAccount, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedAccountId, accountId);
        }

        [Fact]
        public async Task CreateBankAccount_SuccessPass_CallRepositoryUpdateWithRightModel()
        {
            //ARRANGE
            var userId = "userId";
            decimal startAmount = 0;
            AvaliableCurrencies currency = AvaliableCurrencies.RUB;
            BankAccount bankAccount = new BankAccount(userId, currency, startAmount);
            var referenceAccount = new BankAccount(userId, currency, startAmount);
            string expectedAccountId = "accountId";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.CreateAsync(It.IsAny<BankAccount>(), _cancellationToken))
                .ReturnsAsync(expectedAccountId);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var accountId = await _bankAccountService.CreateAsync(bankAccount, _cancellationToken);

            //ASSERT
            _bankAccountRepositoryMock.Verify(repo => repo
            .CreateAsync(It.Is<BankAccount>(x => MyMockExtensions.PublicInstancePropertiesEqual(x, referenceAccount) == true), _cancellationToken));
        }
    }
}
