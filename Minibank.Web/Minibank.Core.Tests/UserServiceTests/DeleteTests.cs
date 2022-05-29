using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Domains.Users.Validators;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.UserServiceTests
{
    public class DeleteTests
    {
        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBankAccountRepository> _bankAccountRepositoryMock;
        private readonly Mock<IUnitOfWork> _saveChangesMock;
        private readonly UserCreationValidator _userCreationValidator;
        private readonly UserUpdateValidator _userUpdateValidator;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public DeleteTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _bankAccountRepositoryMock = new Mock<IBankAccountRepository>();
            _saveChangesMock = new Mock<IUnitOfWork>();
            _userCreationValidator = new UserCreationValidator();
            _userUpdateValidator = new UserUpdateValidator(_userRepositoryMock.Object);

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _userService = new UserService(_userRepositoryMock.Object, _bankAccountRepositoryMock.Object,
                _saveChangesMock.Object, _userCreationValidator, _userUpdateValidator);
        }

        [Fact]
        public async Task DeleteUser_DontExist_ShouldThrowException()
        {
            //ARRANGE
            var id = "id123";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>(() => _userService.DeleteAsync(id, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(User)} с id = {id} не найден", exception.Message);
        }

        [Fact]
        public async Task DeleteUser_HaveAccounts_ShouldThrowException()
        {
            //ARRANGE
            string userId = "id123";
            var user = new User { Id = userId, Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.ExistsByUserIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            //ACT
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _userService.DeleteAsync(userId, _cancellationToken));

            //ASSERT
            Assert.Contains("Пользователь с привязанными аккаунтами не может быть удалён", exception.Message);
        }

        [Fact]
        public async Task UpdateUser_SuccessPass_CallRepositoryDeleteOnce()
        {
            //ARRANGE
            string id = "id123";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.ExistsByUserIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.DeleteAsync(id, _cancellationToken);

            //ASSERT
            _userRepositoryMock.Verify(repository => repository.DeleteAsync(id, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_SuccessPass_CallRepositoryDeleteWithRightId()
        {
            //ARRANGE
            string userId = "id123";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.ExistsByUserIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.DeleteAsync(userId, _cancellationToken);

            //ASSERT
            _userRepositoryMock.Verify(userRepository => userRepository.DeleteAsync(It.Is<string>(x => x == userId), _cancellationToken));
        }

        [Fact]
        public async Task DeleteUser_SuccessPass_SaveChangesOnce()
        {
            //ARRANGE
            string userId = "id123";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.ExistsByUserIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.DeleteAsync(userId, _cancellationToken);

            //ASSERT
            _saveChangesMock.Verify(saver => saver.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task DeleteUser_SuccessPass_ReturnResult(int numberOfDbChanges, bool expectedResult)
        {
            //ARRANGE
            string userId = "id123";
            var user = new User { Id = userId, Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _bankAccountRepositoryMock
                .Setup(bankAccountRepository => bankAccountRepository.ExistsByUserIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(numberOfDbChanges);

            //ACT
            var result = await _userService.DeleteAsync(userId, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedResult, result);
        }
    }
}