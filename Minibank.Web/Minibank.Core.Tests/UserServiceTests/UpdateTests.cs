using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Domains.Users.Validators;
using Minibank.Core.Tests.Utils;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.UserServiceTests
{
    public class UpdateTests
    {
        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBankAccountRepository> _bankAccountRepositoryMock;
        private readonly Mock<IUnitOfWork> _saveChangesMock;
        private readonly UserCreationValidator _userCreationValidator;
        private readonly UserUpdateValidator _userUpdateValidator;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public UpdateTests()
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
        public async Task UpdateUser_DontExist_ShouldThrowException()
        {
            //ARRANGE
            var id = "id123";
            var user = new User { Id = id, Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            //ACT
            var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>
                (() => _userService.UpdateAsync(user, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(User)} с id = {id} не найден", exception.Message);
        }

        [Fact]
        public async Task UpdateUser_SuccessPass_CallRepositoryUpdateOnce()
        {
            //ARRANGE
            string id = "id123";
            var user = new User { Id = id, Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.UpdateAsync(user, _cancellationToken);

            //ASSERT
            _userRepositoryMock.Verify(repository => repository.UpdateAsync(user, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_SuccessPass_CallRepositoryUpdateWithRightModel()
        {
            //ARRANGE
            string id = "id123";
            var user = new User { Id = id, Login = "Login", Email = "123" };
            var referenceUser = new User { Id = user.Id, Login = user.Login, Email = user.Email };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.UpdateAsync(user, _cancellationToken);

            //ASSERT
            _userRepositoryMock.Verify(repo => repo
            .UpdateAsync(It.Is<User>(x => MyMockExtensions.PublicInstancePropertiesEqual(x, referenceUser) == true), _cancellationToken));
        }

        [Fact]
        public async Task UpdateUser_SuccessPass_SaveChangesOnce()
        {
            //ARRANGE
            string id = "id123";
            var user = new User { Id = id, Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.UpdateAsync(user, _cancellationToken);

            //ASSERT
            _saveChangesMock.Verify(saver => saver.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task UpdateUser_SuccessPass_ReturnResult(int numberOfDbChanges, bool expectedResult)
        {
            //ARRANGE
            string id = "id123";
            var user = new User { Id = id, Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(numberOfDbChanges);

            //ACT
            var result = await _userService.UpdateAsync(user, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedResult, result);
        }
    }
}