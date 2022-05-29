using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.Services;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Domains.Users.Validators;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Minibank.Core.Tests.UserServiceTests
{
    public class GetTests
    {
        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBankAccountRepository> _bankAccountRepositoryMock;
        private readonly Mock<IUnitOfWork> _saveChangesMock;
        private readonly UserCreationValidator _userCreationValidator;
        private readonly UserUpdateValidator _userUpdateValidator;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public GetTests()
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
        public async Task GetAllUsers_SuccessPass_ReturnList()
        {
            //ARRANGE
            var listOfUsers = new List<User>();

            for (int i = 0; i < 5; i++)
            {
                listOfUsers.Add(new User { Id = i.ToString(), Login = i.ToString() });
            }

            _userRepositoryMock
                .Setup(userRepository => userRepository.GetAllAsync(_cancellationToken))
                .ReturnsAsync(listOfUsers);

            //ACT
            var result = await _userService.GetAll(_cancellationToken);

            //ASSERT
            Assert.Equal(listOfUsers, result);
        }

        [Fact]
        public async Task GetById_DontExist_ShouldThrowException()
        {
            //ARRANGE
            var id = "id123";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(false);

            //ACT
            var exception = await Assert.ThrowsAsync<ObjectNotFoundException>(() => _userService.GetByIdAsync(id, _cancellationToken));

            //ASSERT
            Assert.Contains($"Объект {nameof(User)} с id = {id} не найден", exception.Message);
        }

        [Fact]
        public async Task GetById_SuccessPass_CallRepositoryGetByIdWithRightId()
        {
            //ARRANGE
            var id = "id123";

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            //ACT
            var result = await _userService.GetByIdAsync(id, _cancellationToken);

            //ASSERT
            _userRepositoryMock.Verify(repo => repo
            .GetByIdAsync(It.Is<string>(x => x == id), _cancellationToken));
        }

        [Fact]
        public async Task GetById_SuccessPass_ReturnUser()
        {
            //ARRANGE
            var id = "id123";
            var user = new User { Id = id, Login = "Login", Email = "Email" };

            _userRepositoryMock
                .Setup(userRepository => userRepository.ExistsAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(true);

            _userRepositoryMock
                .Setup(userRepository => userRepository.GetByIdAsync(It.IsAny<string>(), _cancellationToken))
                .ReturnsAsync(user);

            //ACT
            var result = await _userService.GetByIdAsync(id, _cancellationToken);

            //ASSERT
            Assert.Equal(user, result);
        }
    }
}
