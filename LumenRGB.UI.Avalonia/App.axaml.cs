using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LumenRGB.UI.Avalonia.ViewModels;
using LumenRGB.UI.Avalonia.Views;
using System.Linq;
using Avalonia.Threading;
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
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                var splash = new StartupWindow();
                desktop.MainWindow = splash;
                splash.Show();

                Task.Run(async () =>
                {
                    await Task.Delay(800);

                    splash.ViewModel.StatusText = "Loading modules.";
                    await Task.Delay(800);

                    splash.ViewModel.StatusText = "Initializing UI..";
                    await Task.Delay(800);

                    splash.ViewModel.StatusText = "Starting...";
                    await Task.Delay(800);

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
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}