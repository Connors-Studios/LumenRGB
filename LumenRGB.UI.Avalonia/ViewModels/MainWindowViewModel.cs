using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LumenRGB.UI.Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // The current page/viewmodel being displayed
        [ObservableProperty]
        private object currentPage;

        // The currently selected page name
        [ObservableProperty]
        private string selectedPage;

        public MainWindowViewModel()
        {
            // Set the default page to Home
            SelectedPage = "Home";
            CurrentPage = new HomeViewModel();
        }

        // Home navigation command
        [RelayCommand]
        private void NavigateHome()
        {
            SelectedPage = "Home";
            CurrentPage = new HomeViewModel();
        }

        // Devices navigation command
        [RelayCommand]
        private void NavigateDevices()
        {
            SelectedPage = "Devices";
            CurrentPage = new DevicesViewModel();
        }

        // Settings navigation command
        [RelayCommand]
        private void NavigateSettings()
        {
            SelectedPage = "Settings";
            CurrentPage = new SettingsViewModel();
        }

        // About navigation command
        [RelayCommand]
        private void NavigateAbout()
        {
            SelectedPage = "About";
            CurrentPage = new AboutViewModel();
        }
    }
}
