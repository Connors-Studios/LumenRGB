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

                // Linux cleanup for old AppImage
                TryCleanupOldAppImage();

                var splash = new StartupWindow();
                desktop.MainWindow = splash;
                splash.Show();

                _ = Task.Run(async () =>
                {
                    // --- UPDATE CHECK ---
                    splash.ViewModel.StatusText = "Checking updates";
                    await UpdateChecker.CheckForUpdatesAsync();   // NEW

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

        private void TryCleanupOldAppImage()
        {
            try
            {
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
