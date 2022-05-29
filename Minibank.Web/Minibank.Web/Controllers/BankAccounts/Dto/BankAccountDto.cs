using Minibank.Core.Domains.Currencies;
using System;

namespace Minibank.Web.Controllers.BankAccounts
{
    public class BankAccountDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public AvaliableCurrencies Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime OpeningDate { get; set; }
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime? ClosingDate { get; set; }
    }
}
