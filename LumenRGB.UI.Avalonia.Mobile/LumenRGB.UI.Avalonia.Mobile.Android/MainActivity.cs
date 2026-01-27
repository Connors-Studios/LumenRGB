using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace LumenRGB.UI.Avalonia.Mobile.Android
{
    [Activity(
        Label = "LumenRGB.UI.Avalonia.Mobile.Android",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ColorMode = "dark",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App>
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder)
                .WithInterFont();
        }
    }
}
