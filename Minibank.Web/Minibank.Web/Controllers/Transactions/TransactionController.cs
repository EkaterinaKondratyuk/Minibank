using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Minibank.Core.Domains.Transactions;
using Minibank.Core.Domains.Transactions.Services;
using Minibank.Web.Controllers.Transactions.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Web.Controllers.Transactions
{
    [ApiController]
    [Route("api/v1/minibank/[controller]/[action]")]
    public class TransactionController
    {
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public TransactionController(ITransactionService service, IMapper mapper)
        {
            _transactionService = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<TransactionDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<Transaction> transactions = await _transactionService.GetAll(cancellationToken);
            return _mapper.Map<List<Transaction>, List<TransactionDto>>(transactions);
        }
    }
}
