using Minibank.Core.Domains.Currencies;

namespace Minibank.Web.Controllers.BankAccounts
{
    public class BankAccountDtoPublic
    {
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public AvaliableCurrencies Currency { get; set; }
    }
}