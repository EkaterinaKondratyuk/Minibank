using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Currencies.Services
{
    public interface ICurrencyCourseClient
    {
        Task<List<decimal>> GetCurrencyExchangeRateAsync(List<AvaliableCurrencies> currencyCodes, CancellationToken cancellationToken);
    }
}
