using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Currencies.Services
{
    public interface ICurrencyConverter
    {
        Task<decimal> ConvertCurrencyAsync(decimal amount, AvaliableCurrencies fromCurrencyCode, AvaliableCurrencies toCurrencyCode, CancellationToken cancellationToken);
    }
}
