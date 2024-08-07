using Newtonsoft.Json;

namespace CurrencyTile.TimerTask.AlphaVantage;

internal class ExchangeRateWrapper
{
    [JsonProperty("Realtime Currency Exchange Rate")]
    internal ExchangeRate RealtimeCurrencyExchangeRate { get; set; } = null!;
}

internal class ExchangeRate
{
    [JsonProperty("1. From_Currency Code")]
    internal string FromCurrencyCode { get; set; } = null!;

    [JsonProperty("2. From_Currency Name")]
    internal string FromCurrencyName { get; set; } = null!;

    [JsonProperty("3. To_Currency Code")]
    internal string ToCurrencyCode { get; set; } = null!;

    [JsonProperty("4. To_Currency Name")]
    internal string ToCurrencyName { get; set; } = null!;

    [JsonProperty("5. Exchange Rate")]
    internal string Rate { get; set; } = null!;

    [JsonProperty("6. Last Refreshed")]
    private DateTime _LastRefreshedInternal { get; set; }

    [JsonProperty("7. Time Zone")]
    private string _TimeZoneInternal { get; set; } = null!;

    internal DateTimeOffset LatRefreshed =>
        new(
            _LastRefreshedInternal,
            TimeZoneInfo.FindSystemTimeZoneById(_TimeZoneInternal).BaseUtcOffset
        );

    [JsonProperty("8. Bid Price")]
    internal string BidPrice { get; set; } = null!;

    [JsonProperty("9. Ask Price")]
    internal string AskPrice { get; set; } = null!;
}
