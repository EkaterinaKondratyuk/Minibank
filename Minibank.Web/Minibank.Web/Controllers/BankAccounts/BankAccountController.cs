using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Minibank.Core.Domains.BankAccounts;
using Minibank.Core.Domains.BankAccounts.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Web.Controllers.BankAccounts
{
    [ApiController]
    [Route("api/v1/minibank/[controller]/[action]")]
    public class BankAccountController
    {
        public readonly IBankAccountService _bankAccountService;
        private readonly IMapper _mapper;

        public BankAccountController(IBankAccountService service, IMapper mapper)
        {
            _bankAccountService = service;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<string> CreateAsync(BankAccountDtoPublic outerModel, CancellationToken cancellationToken)
        {
            BankAccount bankAccount = _mapper.Map<BankAccountDtoPublic, BankAccount>(outerModel);
            return await _bankAccountService.CreateAsync(bankAccount, cancellationToken);
        }

        [HttpGet]
        public async Task<List<BankAccountDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<BankAccount> bankAccounts = await _bankAccountService.GetAll(cancellationToken);
            return _mapper.Map<List<BankAccount>, List <BankAccountDto>>(bankAccounts);
        }

        [HttpGet]
        public async Task<BankAccountDto> GetById(string id, CancellationToken cancellationToken)
        {
            BankAccount bankAccount = await _bankAccountService.GetByIdAsync(id, cancellationToken);
            return _mapper.Map<BankAccount, BankAccountDto>(bankAccount);
        }

        [HttpPut("{id}")]
        public async Task<bool> CloseAccountAsync(string id, CancellationToken cancellationToken)
        {
            return await _bankAccountService.CloseAsync(id, cancellationToken);
        }

        [HttpPut]
        public async Task<bool> TransferMoneyAsync(decimal amount, string fromBankAccountId, string toBankAccountId, CancellationToken cancellationToken)
        {
            return await _bankAccountService.TransferMoneyAsync(amount, fromBankAccountId, toBankAccountId, cancellationToken);
        }

        [HttpPut]
        public async Task<decimal> CalculateComissionAsync(decimal amount, string fromBankAccountId, string toBankAccountId, CancellationToken cancellationToken)
        {
            return await _bankAccountService.CalculateComissionAsync(amount, fromBankAccountId, toBankAccountId, cancellationToken);
        }
    }
}
