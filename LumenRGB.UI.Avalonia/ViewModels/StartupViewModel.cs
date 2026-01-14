using CommunityToolkit.Mvvm.ComponentModel;

namespace LumenRGB.UI.Avalonia.ViewModels
{
    public partial class StartupViewModel : ObservableObject
    {
        [ObservableProperty]
        private string statusText = "Starting";
    }
}