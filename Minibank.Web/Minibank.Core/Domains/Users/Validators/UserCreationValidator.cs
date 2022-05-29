using FluentValidation;

namespace Minibank.Core.Domains.Users.Validators
{
    public class UserCreationValidator : AbstractValidator<User>
    {
        public UserCreationValidator()
        {
            RuleFor(x => x.Login).NotEmpty()
                .WithMessage("Введите логин");
            RuleFor(x => x.Login.Length).LessThanOrEqualTo(256)
                .WithMessage("Логин не должен превышать 256 символов");
        }
    }
}
