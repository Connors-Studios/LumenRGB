using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LumenRGB.UI.Avalonia.ViewModels;

namespace LumenRGB.UI.Avalonia;

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