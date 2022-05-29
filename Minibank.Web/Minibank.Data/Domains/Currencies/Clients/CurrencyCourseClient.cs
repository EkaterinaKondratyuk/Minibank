using Microsoft.AspNetCore.Http;
using Minibank.Core.Domains.Currencies;
using Minibank.Core.Domains.Currencies.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Data.Domains.Currencies
{
    public class CurrencyCourseClient : ICurrencyCourseClient
    {
        private readonly HttpClient _httpClient;
        private readonly List<decimal> _result;

        public CurrencyCourseClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _result = new List<decimal>();
        }

        public async Task<List<decimal>> GetCurrencyExchangeRateAsync(List<AvaliableCurrencies> currencyCodes, CancellationToken cancellationToken)
        {
            Task<CourseResponseModel> request = _httpClient.GetFromJsonAsync<CourseResponseModel>("daily_json.js", cancellationToken);
            var response = await request;
            if (request.IsCompletedSuccessfully == false)
                throw new BadHttpRequestException("Не удалось выполнить запрос");

            response.Valute[AvaliableCurrencies.RUB.ToString()] = new ValueItem { Value = 1 };

            foreach (var currency in currencyCodes)
            {
                _result.Add((decimal)Math.Round(response.Valute[currency.ToString()].Value, 2));
            }

            return _result;
        }
    }
}
