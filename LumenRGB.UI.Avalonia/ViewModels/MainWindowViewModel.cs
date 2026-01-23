using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LumenRGB.UI.Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // The current page/viewmodel being displayed
        [ObservableProperty]
        private object currentPage = null!;

        // The currently selected page name
        [ObservableProperty]
        private string selectedPage = null!;

        public MainWindowViewModel()
        {
            // Set the default page to Home
            Navigate("Home");
        }

        [RelayCommand]
        private void Navigate(string page)
        {
            if (SelectedPage == page)
                return;

            SelectedPage = page;

            CurrentPage = page switch
            {
                "Home" => new HomeViewModel(),
                "Devices" => new DevicesViewModel(),
                "Settings" => new SettingsViewModel(),
                "About" => new AboutViewModel(),
                _ => new HomeViewModel(),
            };
        }
    }
}
