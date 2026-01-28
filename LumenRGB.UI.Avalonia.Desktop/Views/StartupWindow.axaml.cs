using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LumenRGB.UI.Avalonia.Desktop.ViewModels;

namespace LumenRGB.UI.Avalonia.Desktop;

public partial class StartupWindow : Window
{
    public StartupViewModel ViewModel { get; }
    public StartupWindow()
    {
        InitializeComponent();
        ViewModel = new StartupViewModel();
        DataContext = ViewModel;
    }
}