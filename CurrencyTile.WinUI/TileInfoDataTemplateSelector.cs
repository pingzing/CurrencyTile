using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CurrencyTile.WinUI;

internal class TileInfoDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ExchangeRate { get; set; }
    public DataTemplate? Quote { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is TileQuoteInfo)
        {
            return Quote;
        }

        if (item is TileExchangeRateInfo)
        {
            return ExchangeRate;
        }

        throw new ArgumentOutOfRangeException(nameof(item));
    }
}
