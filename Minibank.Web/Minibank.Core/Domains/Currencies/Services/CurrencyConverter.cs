using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Currencies.Services
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly ICurrencyCourseClient _currencyCourceProvider;

        public CurrencyConverter(ICurrencyCourseClient currencyCourceProvider)
        {
            _currencyCourceProvider = currencyCourceProvider;
        }

        public async Task<decimal> ConvertCurrencyAsync(decimal amount, AvaliableCurrencies fromCurrencyCode, AvaliableCurrencies toCurrencyCode, CancellationToken cancellationToken)
        {
            if (amount < 0)
                throw new ValidationException("Пожалуйста, введите неотрицательное значение");

            var currencies = new List<AvaliableCurrencies>() { fromCurrencyCode, toCurrencyCode };

            List<decimal> exchangeRates = await _currencyCourceProvider.GetCurrencyExchangeRateAsync(currencies, cancellationToken);

            decimal aimCurrencyAmount = amount / exchangeRates[1] * exchangeRates[0];
            return aimCurrencyAmount;
        }
    }
}