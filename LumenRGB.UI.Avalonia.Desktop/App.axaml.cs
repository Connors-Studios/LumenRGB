using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LumenRGB.UI.Avalonia.Desktop.ViewModels;
using LumenRGB.UI.Avalonia.Desktop.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LumenRGB.UI.Avalonia.Desktop
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Linux clean up for old AppImage
                TryCleanupOldAppImage();

                // Show splash screen
                var splash = new StartupWindow();
                desktop.MainWindow = splash;
                splash.Show();

                // Launch task
                _ = Task.Run(async () =>
                {
                    // Check for updates
                    splash.ViewModel.StatusText = "Checking for updates";
                    await UpdateChecker.CheckForUpdatesAsync();

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

        // Cleans up old AppImage files on Linux systems
        private void TryCleanupOldAppImage()
        {
            try
            {
                // Delete old AppImage if exists
                var exe = Environment.ProcessPath!;
                var old = exe + ".old";
                if (File.Exists(old))
                    File.Delete(old);
            }
            catch
            {
                // Silent fail
            }
        }
    }
}
