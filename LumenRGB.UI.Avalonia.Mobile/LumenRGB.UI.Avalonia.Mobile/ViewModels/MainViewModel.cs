using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace LumenRGB.UI.Avalonia.Mobile.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        // Drawer open/close state
        [ObservableProperty]
        private bool _isMenuOpen;

        // The current page (UserControl) displayed in the content area
        [ObservableProperty]
        private object? _currentPage;

        public MainViewModel()
        {
            // Set default page
            CurrentPage = new HomeViewModel(); // or your actual Home page
        }

        // Toggle the hamburger drawer
        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        // Navigation command used by sidebar buttons
        [RelayCommand]
        private void Navigate(string pageName)
        {
            IsMenuOpen = false; // close drawer when navigating

            CurrentPage = pageName switch
            {
                "Home" => new HomeViewModel(),
                "Devices" => new DevicesViewModel(),
                "Settings" => new SettingsViewModel(),
                "About" => new AboutViewModel(),
                _ => CurrentPage
            };
        }
    }
}
