namespace CurrencyTile.TimerTask;

internal interface IStockQuote
{
    decimal CurrentPrice { get; }

    decimal Change { get; }

    decimal ChangePercent { get; }

    decimal HighPrice { get; }

    decimal LowPrice { get; }

    decimal OpenPrice { get; }

    decimal PreviousClose { get; }

    DateTimeOffset Timestamp { get; }

    string Symbol { get; }
}
