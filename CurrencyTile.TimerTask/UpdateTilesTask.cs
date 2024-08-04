using CurrencyTile.TimerTask.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace CurrencyTile.TimerTask;

public sealed class UpdateTilesTask : IBackgroundTask
{
    private AlphaVantageService _apiService;

    public UpdateTilesTask()
    {
        _apiService = new AlphaVantageService();
    }

    public async void Run(IBackgroundTaskInstance taskInstance)
    {
        var deferral = taskInstance.GetDeferral();

        // TODO: instead of hardcoded updates, just get a list of all secondary tiles, and read their
        // info from their args

        GlobalQuote? vffvxQuote = await GetGlobalQuote("VFFVX");
        if (vffvxQuote != null)
        {
            await UpdateTile("CurrencyTile-VFFVX", vffvxQuote);
        }

        GlobalQuote? vgtQuote = await GetGlobalQuote("VGT");
        if (vgtQuote != null)
        {
            await UpdateTile("CurrencyTile - VGT", vgtQuote);
        }

        ExchangeRate? ethToUsd = await GetExchangeRate("ETH", "USD");
        if (ethToUsd != null)
        {
            // TODO: new UpdateTile overload that takes an exchange rate
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

    private async Task UpdateTile(string tileId, GlobalQuote data)
    {
        if (!SecondaryTile.Exists(tileId))
        {
            // Tile don't exist, bail
            return;
        }
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
                                    {
                                        HintTextStacking = AdaptiveSubgroupTextStacking.Top,
                                        Children =
                                        {
                                            new AdaptiveText
                                            {
                                                HintMaxLines = 1,
                                                HintStyle = AdaptiveTextStyle.Header,
                                                Text = data.Symbol
                                            },
                                            new AdaptiveText
                                            {
                                                HintMaxLines = 1,
                                                HintStyle = AdaptiveTextStyle.SubheaderNumeral,
                                                Text = data.Price
                                            },
                                            new AdaptiveText
                                            {
                                                HintMaxLines = 1,
                                                HintStyle = AdaptiveTextStyle.TitleSubtle,
                                                Text = data.Change
                                            }
                                        }
                                    },
                                    new AdaptiveSubgroup
                                    {
                                        HintTextStacking = AdaptiveSubgroupTextStacking.Bottom,
                                        Children =
                                        {
                                            new AdaptiveText
                                            {
                                                HintMaxLines = 1,
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        var updateManager = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);

        updateManager.Update(new TileNotification(tileContent.GetXml()));

        // Maybe: Update tile args by creating a SecondaryTile with the TileId constructor, setting its properties, then calling Update on it
        var tile = new SecondaryTile(tileId);
        // TODO: Make background color change to green if plus, red if minus
        tile.VisualElements.BackgroundColor = Windows.UI.Color.FromArgb(255, 0, 255, 0);
        tile.Arguments = "most recent data?";
        await tile.UpdateAsync();
    }
}
