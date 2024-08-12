using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CurrencyTile.Shared;
using Microsoft.UI.Xaml;
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CurrencyTile.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public List<string> TileComboBoxOptions { get; } = ["Quote", "ExchangeRate"];

    //private ObservableCollection<TileArgsData> _tiles = new();
    //public ObservableCollection<TileArgsData> Tiles
    //{
    //    get => _tiles;
    //    set
    //    {
    //        if (_tiles != value)
    //        {
    //            _tiles = value;
    //            RaisePropertyChanged();
    //        }
    //    }
    //}

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void myButton_Click(object sender, RoutedEventArgs e)
    {
        var secondaryTile = new SecondaryTile(
            "CurrencyTile-ETHSUD",
            "ETH to USD",
            TileSerializer.SerializeTileArgs(new TileArgsExchangeRate("ETH", "USD")),
            new Uri("ms-appx:///Assets/Square150x150Logo.png"),
            TileSize.Default
        );

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(secondaryTile, hWnd);

        bool created = await secondaryTile.RequestCreateAsync();
        if (created)
        {
            // TODO: Call the update tiles task, ideally with just the args for the new tile
        }
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
            "ExchangeRate" => TileKind.ExchangeRate,
            _ => throw new NotImplementedException()
        };

        if (given != expected)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
