using AutoMapper;
using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.Transactions;
using Minibank.Core.Domains.Users;
using Minibank.Data.Domain.Transactions;
using Minibank.Data.Domain.Users;
using Minibank.Data.Domains.BankAccounts;
using Minibank.Web.Controllers.BankAccounts;
using Minibank.Web.Controllers.Transactions.Dto;
using Minibank.Web.Controllers.Users.Dto;
using System.Collections.Generic;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserDtoPublic, User>();
        CreateMap<User, UserDtoPublic>();
        CreateMap<User, UserDto>();
        CreateMap<UserDbModel, User>();
        CreateMap<User, UserDbModel>();

        CreateMap<BankAccount, BankAccountDto>();
        CreateMap<BankAccountDtoPublic, BankAccount>();
        CreateMap<BankAccountDbModel, BankAccount>();
        CreateMap<BankAccount, BankAccountDbModel>();

        CreateMap<Transaction, TransactionDto>();
        CreateMap<TransactionDbModel, Transaction>();
        CreateMap<Transaction, TransactionDbModel>();
    }
}
