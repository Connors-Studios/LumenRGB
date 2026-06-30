using Avalonia.Controls;
using Avalonia.Labs.Gif;
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

        SplashGif.Source = GifStreamSource.FromUriString(
            "avares://LumenRGB/Assets/LumenRGB_arc_animated.gif");
    }
}