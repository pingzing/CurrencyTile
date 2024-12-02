using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CurrencyTile.Shared;
using CurrencyTile.TimerTask;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Serilog;
using Windows.ApplicationModel.Background;
using Windows.UI;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;
using WinRT.Interop;

namespace CurrencyTile.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    private ILogger _logger = null!;

    // Horrible hack because ApplicationTrigger appears to be broken https://github.com/microsoft/CsWinRT/issues/1518
    // Doesn't throw an exception, but the trigger never fires.
    private UpdateTilesTask _updateTilesTask;

    public event PropertyChangedEventHandler? PropertyChanged;

    public List<string> TileComboBoxOptions { get; } = ["Quote", "Exchange Rate"];

    private ObservableCollection<TileInfo> _tiles = new();
    public ObservableCollection<TileInfo> Tiles
    {
        get => _tiles;
        set
        {
            if (_tiles != value)
            {
                _tiles = value;
                RaisePropertyChanged();
            }
        }
    }

    private Color _inactiveCaptionForegroundColor = Color.FromArgb(255, 133, 133, 133);

    public MainWindow()
    {
        _updateTilesTask = new UpdateTilesTask();
        InitializeComponent();

        // Acrylic title bar
        ExtendsContentIntoTitleBar = true;
        Activated += MainWindow_Activated;
        var titleBar = AppWindow.TitleBar;
        titleBar.ExtendsContentIntoTitleBar = true;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = Colors.White;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveForegroundColor = _inactiveCaptionForegroundColor;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Force the top row of pixels to be inset so that the
        // top border is visible even with ExtendsContentIntoTitleBar true.
        var handle = WindowNative.GetWindowHandle(this);
        MARGINS margins =
            new()
            {
                cxLeftWidth = 0,
                cxRightWidth = 0,
                cyBottomHeight = 0,
                cyTopHeight = 2
            };

        PInvoke.DwmExtendFrameIntoClientArea(new HWND(handle), in margins);
    }

    // Window doesn't have a loaded event, so let's wait until our root UI control--this Grid--is loaded
    private async void Grid_Loaded(object _, RoutedEventArgs __)
    {
        _logger = await new Logging().GetLogger();

        var tiles = await SecondaryTile.FindAllAsync();
        var tileInfoList = tiles.Select(x =>
        {
            var args = TileSerializer.DeserializeTileArgs(x.Arguments);
            TileInfo converted = args switch
            {
                TileArgsQuote quote => new TileQuoteInfo(quote),
                TileArgsExchangeRate rate => new TileExchangeRateInfo(rate),
                _
                    => throw new ArgumentOutOfRangeException(
                        $"Given secondary tile with type {args.GetType().Name} is not supported"
                    )
            };
            return converted;
        });

        Tiles = new ObservableCollection<TileInfo>(tileInfoList);
    }

    public Visibility IsTileType(TileKind expected, object? comboBoxSelection)
    {
        if (comboBoxSelection == null)
        {
            return Visibility.Collapsed;
        }
        string selectedItem = (string)comboBoxSelection;
        TileKind given = selectedItem switch
        {
            "Quote" => TileKind.Quote,
            "Exchange Rate" => TileKind.ExchangeRate,
            _ => throw new NotImplementedException()
        };

        if (given != expected)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    private async void SubmitQuoteButton_Click(object _, RoutedEventArgs __)
    {
        string symbol = QuoteSymbolTextBox.Text;

        var secondaryTile = new SecondaryTile(
            tileId: $"CurrencyTile-{symbol}",
            displayName: symbol,
            arguments: TileSerializer.SerializeTileArgs(new TileArgsQuote(symbol)),
            new Uri("ms-appx:///Assets/Square150x150Logo.png"),
            TileSize.Default
        );

        await PinTile(secondaryTile);
    }

    private async void SubmitExchangeButton_Click(object sender, RoutedEventArgs e)
    {
        string fromCurrency = ExchangeRateFromBox.Text;
        string toCurrency = ExchangeRateToBox.Text;

        var secondaryTile = new SecondaryTile(
            tileId: $"CurrencyTile-{fromCurrency}{toCurrency}",
            displayName: $"{fromCurrency} -> {toCurrency}",
            arguments: TileSerializer.SerializeTileArgs(
                new TileArgsExchangeRate(fromCurrency, toCurrency)
            ),
            new Uri("ms-appx:///Assets/Square150x150Logo.png"),
            TileSize.Default
        );

        await PinTile(secondaryTile);
    }

    ApplicationTrigger _trigger = new ApplicationTrigger();

    private async Task PinTile(SecondaryTile tile)
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(tile, hWnd);

        bool created = await tile.RequestCreateAsync();
        if (created)
        {
            // ApplicationTrigger seems broken. Instead of this:
            //var result = await _trigger.RequestAsync();
            //if (result != ApplicationTriggerResult.Allowed)
            //{
            //    _logger.Error(
            //        "Unable to trigger ApplicationTrigger when pinning tile {tileName}."
            //            + "Received error: {result}",
            //        tile.DisplayName,
            //        result
            //    );
            //}

            // We do this horrifying thing:
            // TODO: Move the BG task logic out into a shared lib so that both the app and
            // the BG task can call it without evilness
            _updateTilesTask.Run(null);
        }
    }

    private void RefreshButton_Click(object _, RoutedEventArgs __) => _updateTilesTask.Run(null);

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Workaround for records not working with x:Bind
public abstract class TileInfo
{
    public TileKind Kind { get; set; }
}

public class TileQuoteInfo(TileArgsQuote quote) : TileInfo
{
    public string Symbol { get; set; } = quote.Symbol;
}

public class TileExchangeRateInfo(TileArgsExchangeRate rate) : TileInfo
{
    public string FromCurrency { get; set; } = rate.FromCurrency;
    public string ToCurrency { get; set; } = rate.ToCurrency;
    public decimal Rate { get; set; }
}
