using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using CurrencyTile.Shared;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using Windows.ApplicationModel.Background;
using Windows.Win32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CurrencyTile.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private const string AppInstanceKey = "primary";

    private Window _mainWindow;
    private ILogger _logger = null!;
    private BackgroundTaskService _bgTaskService;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        _bgTaskService = new BackgroundTaskService();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs _)
    {
        var runningInstance = AppInstance.FindOrRegisterForKey(AppInstanceKey);
        if (!runningInstance.IsCurrent)
        {
            var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            await runningInstance.RedirectActivationToAsync(activationArgs);
            Process.GetCurrentProcess().Kill();
            return;
        }

        _logger = await new Logging().GetLogger();

        AppInstance.GetCurrent().Activated += OnActivated;
        _mainWindow = new MainWindow();
        _mainWindow.Activate();

        _bgTaskService.UnregisterAll();

        var timerTaskRegistration = await _bgTaskService.GetOrRegisterTimerTask();
        var appTriggerTaskRegistration = await _bgTaskService.GetOrRegisterAppTriggerTask();
    }

    private void OnActivated(object? sender, AppActivationArguments e)
    {
        if (e.Data is Windows.ApplicationModel.Activation.LaunchActivatedEventArgs winArgs)
        {
            if (winArgs.TileActivatedInfo != null)
            {
                _mainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    if (_mainWindow.AppWindow.Presenter is OverlappedPresenter presenter)
                    {
                        presenter.Restore(true);
                    }
                });
                // Gotta use Win32 SetForegroundWindow, becaue Window.Activate() uses SetActiveWindow,
                // which won't do anything if the application itself isn't active.
                // We want to Definitely Bring The Window To The Front, Dammit, and that's SetForegroundWindow's
                // job.
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
                PInvoke.SetForegroundWindow(new Windows.Win32.Foundation.HWND(hWnd));
                // Launched via secondary tile, parse out its arguments and do stuff with it
            }
        }
    }
}
