using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LumenRGB.UI.Avalonia.Desktop.ViewModels
{
    public partial class AboutViewModel : ViewModelBase
    {
        // Version information
        [ObservableProperty]
        private string displayVersion = "Version: " + (Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?? "Unknown");

        // Build date information
        [ObservableProperty]
        private string buildDate = "Build Date: " + GetBuildDate();

        // OS information
        [ObservableProperty]
        private string osInfo = "OS: " + GetOSInfo();

        // Runtime information
        [ObservableProperty]
        private string runtimeInfo = "Runtime: " + GetRuntimeInfo();

        // Helper method to get build date and format it as dd/MM/yyyy
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

        // Helper method to get OS information
        private static string GetOSInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";

            return "Unknown OS";
        }

        // Helper method to get runtime information
        private static string GetRuntimeInfo()
        {
            return RuntimeInformation.FrameworkDescription;
        }
    }
}
