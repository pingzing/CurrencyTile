using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppLifecycle;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CurrencyTile.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private const string AppInstanceKey = "primary";

        private Window _mainWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
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

            AppInstance.GetCurrent().Activated += OnActivated;
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
        }

        private void OnActivated(object sender, AppActivationArguments e)
        {
            if (e.Data is Windows.ApplicationModel.Activation.LaunchActivatedEventArgs winArgs)
            {
                if (winArgs.Arguments != "")
                {
                    // Launched via secondary tile, parse out its arguments and do stuff with it
                    (_mainWindow as MainWindow)?.SetMessage($"Activated via: {winArgs.Arguments}");
                }
            }
        }
    }
}
