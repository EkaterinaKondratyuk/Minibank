using System.Text.Json.Serialization;

namespace Minibank.Core.Domains.Currencies
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AvaliableCurrencies
    {
        RUB = 0,
        USD = 1,
        EUR = 2,
    }
}
