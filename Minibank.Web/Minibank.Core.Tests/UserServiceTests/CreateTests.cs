using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Domains.Users.Validators;
using Minibank.Core.Tests.Utils;
using Moq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.UserServiceTests
{
    public class CreateTests
    {
        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBankAccountRepository> _bankAccountRepositoryMock;
        private readonly Mock<IUnitOfWork> _saveChangesMock;
        private readonly UserCreationValidator _userCreationValidator;
        private readonly UserUpdateValidator _userUpdateValidator;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public CreateTests()
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
        public async Task CreateUser_WithNullLogin_ShouldThrowException()
        {
            //ARRANGE
            string expectedId = "id123";
            var user = new User { Login = "", Id = expectedId, Email = "" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.CreateAsync(It.IsAny<User>(), _cancellationToken))
                .ReturnsAsync(expectedId);

            //ACT
            var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>
                (() => _userService.CreateAsync(user, _cancellationToken));

            //ASSERT
            var errors = exception.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}");
            var errorMessage = string.Join(Environment.NewLine, errors);

            Assert.Contains("Введите логин", errorMessage);
        }

        [Fact]
        public async Task CreateUser_WithLoginLongerThan256_ShouldThrowException()
        {
            //ARRANGE
            string expectedId = "id123";
            int loginLength = 257;
            string longLogin = new StringBuilder(loginLength).Insert(0, "s", loginLength).ToString();
            var user = new User { Login = longLogin };

            _userRepositoryMock
                .Setup(userRepository => userRepository.CreateAsync(It.IsAny<User>(), _cancellationToken))
                .ReturnsAsync(expectedId);

            //ACT
            var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>
                (() => _userService.CreateAsync(user, _cancellationToken));

            //ASSERT
            var errors = exception.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}");
            var errorMessage = string.Join(Environment.NewLine, errors);

            Assert.Contains("Логин не должен превышать 256 символов", errorMessage);
        }

        [Fact]
        public async Task CreateUser_SuccessPass_CallRepositoryUpdateWithRightModel()
        {
            //ARRANGE
            string expectedId = "id123";
            var user = new User { Id = expectedId, Login = "Login", Email = "123" };
            var referenceUser = new User { Id = user.Id, Login = user.Login, Email = user.Email };

            _userRepositoryMock
                .Setup(userRepository => userRepository.CreateAsync(It.IsAny<User>(), _cancellationToken))
                .ReturnsAsync(expectedId);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var result = await _userService.CreateAsync(user, _cancellationToken);

            //ASSERT
            _userRepositoryMock.Verify(repo => repo
            .CreateAsync(It.Is<User>(x => MyMockExtensions.PublicInstancePropertiesEqual(x, referenceUser) == true), _cancellationToken));
        }

        [Fact]
        public async Task CreateUser_SuccessPass_ReturnUserId()
        {
            //ARRANGE
            string expectedId = "id123";
            var user = new User { Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.CreateAsync(It.IsAny<User>(), _cancellationToken))
                .ReturnsAsync(expectedId);

            //ACT
            var userId = await _userService.CreateAsync(user, _cancellationToken);

            //ASSERT
            Assert.Equal(expectedId, userId);
        }

        [Fact]
        public async Task CreateUser_SuccessPass_SaveChangesOnce()
        {
            //ARRANGE
            string expectedId = "id123";
            var user = new User { Login = "Login" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.CreateAsync(It.IsAny<User>(), _cancellationToken))
                .ReturnsAsync(expectedId);

            _saveChangesMock
                .Setup(saver => saver.SaveChangesAsync(_cancellationToken))
                .ReturnsAsync(1);

            //ACT
            var userId = await _userService.CreateAsync(user, _cancellationToken);

            //ASSERT
            _saveChangesMock.Verify(saver => saver.SaveChangesAsync(_cancellationToken), Times.Once);
        }
    }
}