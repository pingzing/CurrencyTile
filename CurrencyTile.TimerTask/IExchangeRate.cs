namespace CurrencyTile.TimerTask;

internal interface IExchangeRate
{
    string From { get; }
    string To { get; }
    DateTimeOffset Timestamp { get; }
    decimal Rate { get; }
}
