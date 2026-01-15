using CommunityToolkit.Mvvm.ComponentModel;

namespace LumenRGB.UI.Avalonia.ViewModels
{
    public partial class StartupViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string statusText = "Starting";
    }
}