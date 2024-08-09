namespace CurrencyTile.TimerTask.CurrencyBeacon;

internal class ExchangeRate : IExchangeRate
{
    public long Timestamp { get; set; }
    public string From { get; set; } = null!;
    public string To { get; set; } = null!;
    public decimal Value { get; set; }

    // IExchangeRate implementation
    string IExchangeRate.From => From;
    string IExchangeRate.To => To;
    DateTimeOffset IExchangeRate.Timestamp => DateTimeOffset.FromUnixTimeSeconds(Timestamp);
    decimal IExchangeRate.Rate => Value;
}
