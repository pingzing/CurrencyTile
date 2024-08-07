using Newtonsoft.Json;

namespace CurrencyTile.TimerTask.Finnhub;

internal class StockQuote : IStockQuote
{
    [JsonProperty("c")]
    public decimal CurrentPrice { get; init; }

    [JsonProperty("d")]
    public decimal Change { get; init; }

    [JsonProperty("dp")]
    public decimal ChangePercent { get; init; }

    [JsonProperty("h")]
    public decimal HighPrice { get; init; }

    [JsonProperty("l")]
    public decimal LowPrice { get; init; }

    [JsonProperty("o")]
    public decimal OpenPrice { get; init; }

    [JsonProperty("pc")]
    public decimal PreviousClose { get; init; }

    [JsonProperty("t")]
    private long _Timestamp { get; init; }

    public DateTimeOffset Timestamp => DateTimeOffset.FromUnixTimeSeconds(_Timestamp);

    public string Symbol { get; set; } = null!;
}
