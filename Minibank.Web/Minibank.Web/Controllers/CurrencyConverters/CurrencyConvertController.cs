using Microsoft.AspNetCore.Mvc;
using Minibank.Core.Domains.Currencies;
using Minibank.Core.Domains.Currencies.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Web.Controllers
{
    [ApiController]
    [Route("api/v1/minibank/[controller]/[action]")]
    public class CurrencyConvertController : ControllerBase
    {
        private readonly ICurrencyConverter _currencyConverter;

        public CurrencyConvertController(ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }

        [HttpGet]
        public async Task<decimal> ConvertCurrencyAsync(decimal amount, AvaliableCurrencies fromCurrencyCode,
            AvaliableCurrencies toCurrencyCode, CancellationToken cancellationToken)
        {
            return await _currencyConverter.ConvertCurrencyAsync(amount, fromCurrencyCode, toCurrencyCode, cancellationToken);
        }
    }
}
