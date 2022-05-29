using FluentValidation;
using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Core.Domains.Users.Validators;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Users.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<User> _userCreationValidator;
        private readonly IValidator<User> _userUpdateValidator;

        public UserService(IUserRepository userRepository, IBankAccountRepository bankAccountRepository,
            IUnitOfWork unitOfWork, UserCreationValidator userCreationValidator, UserUpdateValidator userUpdateValidator)
        {
            _userRepository = userRepository;
            _bankAccountRepository = bankAccountRepository;
            _unitOfWork = unitOfWork;
            _userCreationValidator = userCreationValidator;
            _userUpdateValidator = userUpdateValidator;
        }

        public async Task<string> CreateAsync(User user, CancellationToken cancellationToken)
        {
            await _userCreationValidator.ValidateAndThrowAsync(user, cancellationToken);
            string id = await _userRepository.CreateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return id;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            await ValidateUserRemovalAndThrowAsync(id, cancellationToken);
            await _userRepository.DeleteAsync(id, cancellationToken);
            int numberOfDbChanges = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return numberOfDbChanges > 0;
        }

        public Task<List<User>> GetAll(CancellationToken cancellationToken)
        {
            return _userRepository.GetAllAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            await _userUpdateValidator.ValidateAndThrowAsync(user, cancellationToken);
            await _userRepository.UpdateAsync(user, cancellationToken);
            int numberOfDbChanges = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (numberOfDbChanges > 0);
        }

        public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (await _userRepository.ExistsAsync(id, cancellationToken) == false)
                throw new ObjectNotFoundException($"Объект {nameof(User)} с id = {id} не найден");
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        private async Task ValidateUserRemovalAndThrowAsync(string id, CancellationToken cancellationToken)
        {
            if (await _userRepository.ExistsAsync(id, cancellationToken) == false)
                throw new ObjectNotFoundException($"Объект {nameof(User)} с id = {id} не найден");
            if (await _bankAccountRepository.ExistsByUserIdAsync(id, cancellationToken) == true)
                throw new ValidationException("Пользователь с привязанными аккаунтами не может быть удалён");
        }
    }
}
