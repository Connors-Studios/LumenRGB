using CommunityToolkit.Mvvm.ComponentModel;

namespace LumenRGB.UI.Avalonia.Mobile.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _greeting = "Welcome to Avalonia!";
    }
}
