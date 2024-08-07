namespace CurrencyTile.Shared;

public enum TileKind
{
    Quote = 0,
    ExchangeRate = 1
};

public abstract record TileArgsData(TileKind Kind);

public record TileArgsQuote(TileKind Kind, string Symbol) : TileArgsData(Kind);

public record TileArgsExchangeRate(TileKind Kind, string FromCurrency, string ToCurrency)
    : TileArgsData(Kind);

public class TileSerializer
{
    /// <summary>
    /// Converts the TileArgsData to a string that can be used as the arguments for a secondary tile
    /// </summary>
    public static string SerializeTileArgs(TileArgsData tileArgs)
    {
        if (tileArgs is TileArgsQuote quote)
        {
            return $"0|{quote.Symbol}";
        }
        else if (tileArgs is TileArgsExchangeRate rate)
        {
            return $"1|{rate.FromCurrency}|{rate.ToCurrency}";
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                nameof(tileArgs),
                $"The given tileArgs type of '{tileArgs.GetType().FullName}' is unsupported."
            );
        }
    }

    /// <summary>
    /// Converts the arguments found in a secondary tile's 'Arguments' into a <see cref="TileArgsData"/>.
    /// </summary>
    public static TileArgsData DeserializeTileArgs(string tileArgs)
    {
        string[] parts = tileArgs.Split('|');
        TileKind kind = (TileKind)int.Parse(parts[0]);

        if (kind == TileKind.Quote)
        {
            return new TileArgsQuote(kind, parts[1]);
        }
        if (kind == TileKind.ExchangeRate)
        {
            return new TileArgsExchangeRate(kind, parts[1], parts[2]);
        }

        throw new ArgumentOutOfRangeException(
            nameof(tileArgs),
            "The given TileKind of 'kind' is unsupported."
        );
    }
}
