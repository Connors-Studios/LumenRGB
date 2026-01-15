using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LumenRGB.UI.Avalonia.ViewModels;
using LumenRGB.UI.Avalonia.Views;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using System.Linq;
using System.Threading.Tasks;

namespace LumenRGB.UI.Avalonia
{
    public partial class App : Application
    {
        private SparkleUpdater? _sparkle;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                var splash = new StartupWindow();
                desktop.MainWindow = splash;
                splash.Show();

                // Start background startup tasks
                Task.Run(async () =>
                {
                    // --- UPDATE CHECK ---
                    splash.ViewModel.StatusText = "Checking updates";

                    _sparkle = new SparkleUpdater(
                        "https://github.com/Connors-Studios/LumenRGB/releases/latest/download/appcast.xml",
                        new Ed25519Checker(SecurityMode.Unsafe, "") // replace with your real key later
                    )
                    {
                        UserInteractionMode = UserInteractionMode.DownloadAndInstall,
                        RelaunchAfterUpdate = true,
                        UIFactory = null // REQUIRED for Avalonia (prevents WPF UI)
                    };

                    // REQUIRED when UIFactory = null
                    _sparkle.CloseApplication += () =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                            {
                                desktopLifetime.Shutdown();
                            }
                        });
                    };

                    _sparkle.StartLoop(true);

                    // --- SPLASH SEQUENCE ---
                    await Task.Delay(800);
                    splash.ViewModel.StatusText = "Loading modules.";
                    await Task.Delay(800);

                    splash.ViewModel.StatusText = "Initializing UI..";
                    await Task.Delay(800);

                    splash.ViewModel.StatusText = "Starting...";
                    await Task.Delay(800);

                    // Switch to main window
                    Dispatcher.UIThread.Post(() =>
                    {
                        var main = new MainWindow
                        {
                            DataContext = new MainWindowViewModel()
                        };

                        desktop.MainWindow = main;
                        main.Show();
                        splash.Close();
                    });
                });
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var toRemove = BindingPlugins.DataValidators
                .OfType<DataAnnotationsValidationPlugin>()
                .ToArray();

            foreach (var plugin in toRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
