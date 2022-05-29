using FluentValidation;
using Minibank.Core.Domains.Users.UserRepository;

namespace Minibank.Core.Domains.Users.Validators
{
    public class UserUpdateValidator : AbstractValidator<User>
    {
        public UserUpdateValidator(IUserRepository userRepository)
        {
            RuleFor(x => x).MustAsync((user, cancellationToken) => userRepository.ExistsAsync(user.Id, cancellationToken))
                .WithMessage(x => $"Объект {nameof(User)} с id = {x.Id} не найден");
        }
    }
}
