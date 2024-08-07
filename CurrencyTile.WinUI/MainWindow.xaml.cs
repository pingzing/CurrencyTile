using System;
using CurrencyTile.Shared;
using Microsoft.UI.Xaml;
using Windows.UI.StartScreen;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CurrencyTile.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private async void myButton_Click(object sender, RoutedEventArgs e)
    {
        var secondaryTile = new SecondaryTile(
            "CurrencyTile-VFFVX",
            "VFFVX",
            TileSerializer.SerializeTileArgs(new TileArgsQuote(TileKind.Quote, "VFFVX")),
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

    public void SetMessage(string message)
    {
        DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
            () =>
            {
                MessageBlock.Text = message;
            }
        );
    }
}
