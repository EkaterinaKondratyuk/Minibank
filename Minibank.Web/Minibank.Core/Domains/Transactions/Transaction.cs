using Minibank.Core.Domains.Currencies;
using System;

namespace Minibank.Core.Domains.Transactions
{
    public class Transaction
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public AvaliableCurrencies Currency { get; set; }
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }
        public DateTime CreatedAt { get; set; }

        private Transaction()
        {
        }

        public Transaction(decimal amount, AvaliableCurrencies currency, string fromAccountId, string toAccountId)
        {
            Amount = amount;
            Currency = currency;
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
        }
    }
}
