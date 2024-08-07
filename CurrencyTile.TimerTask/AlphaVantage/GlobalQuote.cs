using Newtonsoft.Json;

namespace CurrencyTile.TimerTask.AlphaVantage;

internal class GlobalQuoteWrapper
{
    [JsonProperty("Global Quote")]
    internal GlobalQuote GlobalQuote { get; set; } = null!;
}

internal class GlobalQuote : IStockQuote
{
    [JsonProperty("01. symbol")]
    internal string Symbol { get; set; } = null!;

    /// <summary>
    /// Price in USD with four digits of precision.
    /// </summary>
    [JsonProperty("02. open")]
    internal string Open { get; set; } = null!;

    /// <summary>
    /// Price in USD with four digits of precision.
    /// </summary>
    [JsonProperty("03. high")]
    internal string High { get; set; } = null!;

    /// <summary>
    /// Price in USD with four digits of precision.
    /// </summary>
    [JsonProperty("04. low")]
    internal string Low { get; set; } = null!;

    /// <summary>
    /// Price in USD with four digits of precision.
    /// </summary>
    [JsonProperty("05. price")]
    internal string Price { get; set; } = null!;

    /// <summary>
    /// Price in USD, but as a numeric value.
    /// </summary>
    internal decimal PriceDecimal => decimal.Parse(Price);

    /// <summary>
    /// Price in USD, with no cents(?)
    /// </summary>
    [JsonProperty("06. volume")]
    internal string Volume { get; set; } = null!;

    [JsonProperty("07. latest trading day")]
    internal DateOnly LatestTradingDay { get; set; }

    [JsonProperty("08. previous close")]
    internal string PreviousClose { get; set; } = null!;

    /// <summary>
    /// Change in USD, from previous close, with four digits of precision.
    /// Can be negative.
    /// </summary>
    [JsonProperty("09. change")]
    internal string Change { get; set; } = null!;

    /// <summary>
    /// Change in decimal, but as a numeric value.
    /// </summary>
    internal decimal ChangeDecimal => decimal.Parse(Change);

    [JsonProperty("10. change percent")]
    internal string ChangePercent { get; set; } = null!;

    // Interface implementation.
    decimal IStockQuote.CurrentPrice => PriceDecimal;

    decimal IStockQuote.Change => ChangeDecimal;

    decimal IStockQuote.ChangePercent => decimal.Parse(ChangePercent);

    decimal IStockQuote.HighPrice => decimal.Parse(High);

    decimal IStockQuote.LowPrice => decimal.Parse(Low);

    decimal IStockQuote.OpenPrice => decimal.Parse(Open);

    decimal IStockQuote.PreviousClose => decimal.Parse(PreviousClose);

    DateTimeOffset IStockQuote.Timestamp => new(LatestTradingDay, TimeOnly.MinValue, TimeSpan.Zero);

    string IStockQuote.Symbol => Symbol;
}
