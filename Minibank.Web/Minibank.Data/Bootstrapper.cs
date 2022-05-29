using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minibank.Core;
using Minibank.Core.Domains.BankAccounts.Repositories;
using Minibank.Core.Domains.Currencies.Services;
using Minibank.Core.Domains.Transactions.Repositories;
using Minibank.Core.Domains.Users.UserRepository;
using Minibank.Data.Domain.Transactions.Repositories;
using Minibank.Data.Domain.Users.Repositories;
using Minibank.Data.Domains.BankAccounts.Repositories;
using Minibank.Data.Domains.Currencies;
using System;

namespace Minibank.Data
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ICurrencyCourseClient, CurrencyCourseClient>(options =>
            {
                options.BaseAddress = new Uri(configuration["SomeUri"]);
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            return services;
        }
    }
}