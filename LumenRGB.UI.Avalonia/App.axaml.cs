using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LumenRGB.UI.Avalonia.ViewModels;
using LumenRGB.UI.Avalonia.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LumenRGB.UI.Avalonia
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
                DisableAvaloniaDataAnnotationValidation();

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
