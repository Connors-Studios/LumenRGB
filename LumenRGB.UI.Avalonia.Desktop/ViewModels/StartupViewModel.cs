using CommunityToolkit.Mvvm.ComponentModel;

namespace LumenRGB.UI.Avalonia.Desktop.ViewModels
{
    public partial class StartupViewModel : ViewModelBase
    {
        // Status text displayed on the startup screen
        [ObservableProperty]
        private string statusText = "Starting";
    }
}