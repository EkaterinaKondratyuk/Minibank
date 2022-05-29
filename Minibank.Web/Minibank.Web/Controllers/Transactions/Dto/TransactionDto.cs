using Minibank.Core.Domains.Currencies;
using System;

namespace Minibank.Web.Controllers.Transactions.Dto
{
    public class TransactionDto
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public AvaliableCurrencies Currency { get; set; }
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
