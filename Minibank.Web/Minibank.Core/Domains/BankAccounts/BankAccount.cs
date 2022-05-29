using Minibank.Core.Domains.Currencies;
using System;

namespace Minibank.Core.Domains.BankAccounts
{
    public class BankAccount
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public AvaliableCurrencies Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        private BankAccount()
        {
        }

        public BankAccount(string userId, AvaliableCurrencies currency, decimal balance)
        {
            UserId = userId;
            Balance = balance;
            Currency = currency;
        }
    }
}
