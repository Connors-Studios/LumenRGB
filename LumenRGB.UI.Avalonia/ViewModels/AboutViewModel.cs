using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LumenRGB.UI.Avalonia.ViewModels
{
    public partial class AboutViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string displayVersion =
            "Version: " +
            (Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? "Unknown");

        [ObservableProperty]
        private string buildDate =
            "Build Date: " +
            GetBuildDate();

        [ObservableProperty]
        private string osInfo =
            "OS: " + GetOSInfo();

        [ObservableProperty]
        private string runtimeInfo =
            "Runtime: " + GetRuntimeInfo();

        private static string GetBuildDate()
        {
            try
            {
                var path = AppContext.BaseDirectory;

                if (string.IsNullOrWhiteSpace(path))
                    return "Unknown";

                var date = Directory.GetLastWriteTime(path);
                return date.ToString("dd/MM/yyyy");
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetOSInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";

            return "Unknown OS";
        }

        private static string GetRuntimeInfo()
        {
            return RuntimeInformation.FrameworkDescription;
        }
    }
}
