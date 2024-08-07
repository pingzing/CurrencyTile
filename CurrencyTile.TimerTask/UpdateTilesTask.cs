using System.Diagnostics;
using CurrencyTile.Shared;
using CurrencyTile.TimerTask.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace CurrencyTile.TimerTask;

public sealed class UpdateTilesTask : IBackgroundTask
{
    private IAlphaVantageService _apiService;

    public UpdateTilesTask()
    {
#if DEBUG
        _apiService = new DummyAlphaVantageService();
#else
        _apiService = new AlphaVantageService();
#endif
    }

    public async void Run(IBackgroundTaskInstance taskInstance)
    {
        var deferral = taskInstance.GetDeferral();

        var allTiles = await SecondaryTile.FindAllAsync();

        foreach (var tile in allTiles)
        {
            TileArgsData tileArgs = TileSerializer.DeserializeTileArgs(tile.Arguments);
            if (tileArgs is TileArgsQuote quoteArgs)
            {
                GlobalQuote? quote = await GetGlobalQuote(quoteArgs.Symbol);
                if (quote != null)
                {
                    await UpdateTile(tile.TileId, quote);
                }
            }
            else if (tileArgs is TileArgsExchangeRate rateArgs)
            {
                ExchangeRate? currToCurr = await GetExchangeRate(
                    rateArgs.FromCurrency,
                    rateArgs.ToCurrency
                );
                if (currToCurr != null)
                {
                    await UpdateTile(tile.TileId, currToCurr);
                }
            }
        }

        // Stuff the data into storage, so the foreground app can use it too, if it's open
        // use Windows.Storage.ApplicationData.Current because we're packaged, and can just stuff settings in there

        deferral.Complete();
    }

    private Task<GlobalQuote?> GetGlobalQuote(string symbol)
    {
        return _apiService.GetGlobalQuote(symbol);
    }

    private Task<ExchangeRate?> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        return _apiService.GetExchangeRate(fromCurrency, toCurrency);
    }

    private Task UpdateTile(string tileId, GlobalQuote quote) =>
        UpdateTileShared(tileId, GenerateTileContent(quote));

    private Task UpdateTile(string tileId, ExchangeRate rate) =>
        UpdateTileShared(tileId, GenerateTileContent(rate));

    private enum Change
    {
        Positive,
        Negative
    };

    private async Task UpdateTileShared(
        string tileId,
        (TileContent tileContent, Change changeDirection) updateInfo
    )
    {
        if (!SecondaryTile.Exists(tileId))
        {
            // Tile don't exist, bail
            return;
        }

        var updateManager = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);
        var xmlContent = updateInfo.tileContent.GetXml();
        updateManager.Update(new TileNotification(xmlContent));

        var tile = new SecondaryTile(tileId);
        if (updateInfo.changeDirection == Change.Positive)
        {
            tile.VisualElements.BackgroundColor = Windows.UI.Color.FromArgb(255, 0, 255, 0);
        }
        else
        {
            tile.VisualElements.BackgroundColor = Windows.UI.Color.FromArgb(255, 255, 0, 0);
        }
        bool updateSuccess = await tile.UpdateAsync();
        Debug.WriteLine($"Updating ${tileId} result was: {updateSuccess}");
    }

    private const string DownArrow = "🠟"; // This is a unicode "Heavy Downwards Arrow large Equilateral Arrowhead" character
    private const string UpArrow = "🠝"; // And likewise, this is a "Heavy Upwards Arrow Large Equilateral arrowhead" character

    private (TileContent tileContent, Change changeDirection) GenerateTileContent(GlobalQuote quote)
    {
        // Tiles have pretty limited space, especially after our up/down arrow character.
        // We need to truncate their prices to fit, but prioritize the non-decimal part of the price
        // So do some calculation to truncate the price strings appropriately
        const int maxSmallDigits = 4; // Room for six characters. We'll always have an arrow and a decimal point, so four total.
        const int maxMediumDeigits = 8; // Ditto here. Technically room for 10, but minus two for arrow + decimal.
        decimal absolute = Math.Abs(quote.PriceDecimal);
        int nonDecimalDigitCount =
            absolute < 1 ? 0 : (int)(Math.Log10(decimal.ToDouble(absolute)) + 1);

        string prefixArrow = quote.ChangeDecimal < 0 ? DownArrow : UpArrow;

        int maxSmallDecimalDigits = Math.Max(0, maxSmallDigits - nonDecimalDigitCount);
        string smallTilePrice =
            $"{prefixArrow}{quote.PriceDecimal.ToString($"F{maxSmallDecimalDigits}")}";

        int maxMediumDecimalDigits = Math.Max(0, maxMediumDeigits - nonDecimalDigitCount);
        string mediumTilePrice =
            $"{prefixArrow}{quote.PriceDecimal.ToString($"F{maxMediumDecimalDigits}")}";
        // Technically, the medium tile's "change" line can hold an extra digit or two because it uses a smaller font size
        // and might not have a leading negative sign... but, meh.

        var tileContent = new TileContent
        {
            Visual = new TileVisual
            {
                Branding = TileBranding.Name,
                DisplayName = quote.LastestTradingDay.ToShortDateString(),
                TileSmall = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                        {
                            new AdaptiveText { Text = quote.Symbol },
                            new AdaptiveText { Text = smallTilePrice },
                        }
                    }
                },
                TileMedium = new TileBinding
                {
                    Branding = TileBranding.Name,
                    DisplayName = quote.LastestTradingDay.ToShortDateString(),
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                HintMaxLines = 1,
                                HintStyle = AdaptiveTextStyle.Base,
                                Text = quote.Symbol
                            },
                            new AdaptiveText
                            {
                                HintMaxLines = 1,
                                HintStyle = AdaptiveTextStyle.Base,
                                Text = mediumTilePrice
                            },
                            new AdaptiveText
                            {
                                HintMaxLines = 1,
                                HintStyle = AdaptiveTextStyle.BodySubtle,
                                Text = quote.Change
                            }
                        }
                    }
                }
            }
        };

        return (tileContent, quote.ChangeDecimal < 0 ? Change.Negative : Change.Positive);
    }

    private (TileContent tileContent, Change changeDirection) GenerateTileContent(ExchangeRate rate)
    {
        var tileContent = new TileContent
        {
            Visual = new TileVisual
            {
                TileMedium = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        TextStacking = TileTextStacking.Top,
                        Children =
                        {
                            new AdaptiveGroup
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup
                                    { /* TODO */
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return (tileContent, Change.Positive);
    }
}
