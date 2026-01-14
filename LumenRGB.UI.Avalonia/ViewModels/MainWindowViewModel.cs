using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LumenRGB.UI.Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object currentPage;

        [ObservableProperty]
        private string selectedPage;

        public MainWindowViewModel()
        {
            SelectedPage = "Home";
            CurrentPage = new HomeViewModel();
        }

        [RelayCommand]
        private void NavigateHome()
        {
            SelectedPage = "Home";
            CurrentPage = new HomeViewModel();
        }

        [RelayCommand]
        private void NavigateDevices()
        {
            SelectedPage = "Devices";
            CurrentPage = new DevicesViewModel();
        }

        [RelayCommand]
        private void NavigateSettings()
        {
            SelectedPage = "Settings";
            CurrentPage = new SettingsViewModel();
        }

        [RelayCommand]
        private void NavigateAbout()
        {
            SelectedPage = "About";
            CurrentPage = new AboutViewModel();
        }
    }
}
