using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Minibank.Core.Domains.BankAccounts.Services;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Core.Domains.Users.Services;

namespace Minibank.Core
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<ICurrencyConverter, CurrencyConverter>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBankAccountService, BankAccountService>();

            services.AddValidatorsFromAssembly(typeof(Bootstrapper).Assembly);

            return services;
        }
    }
}
