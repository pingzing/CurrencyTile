using System.Diagnostics;
using CurrencyTile.Shared;
using CurrencyTile.TimerTask.CurrencyBeacon;
using CurrencyTile.TimerTask.FinancialModelingPrep;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace CurrencyTile.TimerTask;

public sealed class UpdateTilesTask : IBackgroundTask
{
    private FinancialModelingPrepService _fmpService;
    private CurrencyBeaconService _currencyBeaconService;

    public UpdateTilesTask()
    {
        // TODO: Implement CurrencyBeacon API for exchange rates
        _fmpService = new FinancialModelingPrepService();
        _currencyBeaconService = new CurrencyBeaconService();
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
                IStockQuote? quote = await _fmpService.GetQuote(quoteArgs.Symbol);
                if (quote != null)
                {
                    await UpdateTile(tile.TileId, quote);
                }
            }
            else if (tileArgs is TileArgsExchangeRate rateArgs)
            {
                IExchangeRate? currToCurr = await _currencyBeaconService.GetExchangeRate(
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

    private Task UpdateTile(string tileId, IStockQuote quote) =>
        UpdateTileShared(tileId, GenerateTileContent(quote));

    private Task UpdateTile(string tileId, IExchangeRate rate) =>
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
    const string RightArrow = "🠞"; // This is--you guessed it!--"Heavy Rightwards Arrow with Large Equilateral Arrowhead".

    private (TileContent tileContent, Change changeDirection) GenerateTileContent(IStockQuote quote)
    {
        // Tiles have pretty limited space, especially after our up/down arrow character.
        // We need to truncate their prices to fit, but prioritize the non-decimal part of the price
        // So do some calculation to truncate the price strings appropriately
        const int maxSmallDigits = 4; // Room for six characters. We'll always have an arrow and a decimal point, so four total.
        const int maxMediumDeigits = 8; // Ditto here. Technically room for 10, but minus two for arrow + decimal.
        decimal absolute = Math.Abs(quote.CurrentPrice);
        int nonDecimalDigitCount =
            absolute < 1 ? 0 : (int)(Math.Log10(decimal.ToDouble(absolute)) + 1);

        string prefixArrow = quote.Change < 0 ? DownArrow : UpArrow;

        int maxSmallDecimalDigits = Math.Max(0, maxSmallDigits - nonDecimalDigitCount);
        maxSmallDecimalDigits = Math.Min(maxSmallDecimalDigits, 2); // And cap it at 2, because more is hard to read
        string smallTilePrice =
            $"{prefixArrow}{quote.CurrentPrice.ToString($"F{maxSmallDecimalDigits}")}";

        int maxMediumDecimalDigits = Math.Max(0, maxMediumDeigits - nonDecimalDigitCount);
        maxMediumDecimalDigits = Math.Min(maxMediumDecimalDigits, 2); // And cap it at 2, because more is hard to read
        string mediumTilePrice =
            $"{prefixArrow}{quote.CurrentPrice.ToString($"F{maxMediumDecimalDigits}")}";
        // Technically, the medium tile's "change" line can hold an extra digit or two because it uses a smaller font size
        // and might not have a leading negative sign... but, meh.

        var tileContent = new TileContent
        {
            Visual = new TileVisual
            {
                Branding = TileBranding.Name,
                DisplayName = quote.Timestamp.UtcDateTime.ToShortDateString(),
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
                    DisplayName = quote.Timestamp.UtcDateTime.ToShortDateString(),
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
                                // + 1 digit here because these numbers are usually smaller, and the extra
                                // decimal digit is helpful
                                Text = quote.Change.ToString($"F{maxMediumDecimalDigits + 1}")
                            }
                        }
                    }
                }
            }
        };

        return (tileContent, quote.Change < 0 ? Change.Negative : Change.Positive);
    }

    private (TileContent tileContent, Change changeDirection) GenerateTileContent(
        IExchangeRate rate
    )
    {
        var tileContent = new TileContent
        {
            Visual = new TileVisual
            {
                Branding = TileBranding.Name,
                DisplayName = rate.Timestamp.UtcDateTime.ToShortDateString(),
                TileSmall = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                        {
                            new AdaptiveText { Text = $"{rate.From}{rate.To}" },
                            new AdaptiveText { Text = rate.Rate.ToString("F2") }
                        }
                    }
                },
                TileMedium = new TileBinding
                {
                    Branding = TileBranding.Name,
                    DisplayName = rate.Timestamp.UtcDateTime.ToShortDateString(),
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                HintMaxLines = 1,
                                HintStyle = AdaptiveTextStyle.Base,
                                Text = $"{rate.From} {RightArrow} {rate.To}"
                            },
                            new AdaptiveText
                            {
                                HintMaxLines = 1,
                                HintStyle = AdaptiveTextStyle.Base,
                                Text = rate.Rate.ToString("F3")
                            }
                        }
                    }
                }
            }
        };

        return (tileContent, Change.Positive);
    }
}
