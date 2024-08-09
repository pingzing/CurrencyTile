namespace CurrencyTile.TimerTask.FinancialModelingPrep;

internal class Quote : IStockQuote
{
    public string Symbol { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal ChangesPercentage { get; set; }
    public decimal Change { get; set; }
    public decimal DayLow { get; set; }
    public decimal DayHigh { get; set; }
    public decimal YearHigh { get; set; }
    public decimal YearLow { get; set; }
    public long MarketCap { get; set; }
    public decimal PriceAvg50 { get; set; }
    public decimal PriceAvg200 { get; set; }
    public string? Exchange { get; set; }
    public int Volume { get; set; }
    public int AvgVolume { get; set; }
    public decimal Open { get; set; }
    public decimal PreviousClose { get; set; }
    public decimal Eps { get; set; }
    public decimal Pe { get; set; }
    public long SharesOutstanding { get; set; }
    public long Timestamp { get; set; }

    // IStockQuote implementation

    decimal IStockQuote.CurrentPrice => Price;
    decimal IStockQuote.Change => Change;
    decimal IStockQuote.ChangePercent => ChangesPercentage;
    decimal IStockQuote.HighPrice => DayHigh;
    decimal IStockQuote.LowPrice => DayLow;
    decimal IStockQuote.OpenPrice => Open;
    decimal IStockQuote.PreviousClose => PreviousClose;
    DateTimeOffset IStockQuote.Timestamp => DateTimeOffset.FromUnixTimeSeconds(Timestamp);
    string IStockQuote.Symbol => Symbol;
}
