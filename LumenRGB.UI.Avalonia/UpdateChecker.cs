using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace LumenRGB.UI.Avalonia
{
    public enum UpdateChannel
    {
        Stable,
        Beta
    }

    public static class UpdateChecker
    {
        private static readonly HttpClient _http = new();

        // Default channel (you can change this or load from settings)
        public static UpdateChannel Channel { get; set; } = UpdateChannel.Stable;

        private static string GetManifestUrl()
        {
            return Channel switch
            {
                UpdateChannel.Beta =>
                    "https://github.com/Connors-Studios/LumenRGB/releases/latest/download/update-beta.json",

                UpdateChannel.Stable =>
                    "https://github.com/Connors-Studios/LumenRGB/releases/latest/download/update-stable.json",

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task CheckForUpdatesAsync()
        {
            try
            {
                var url = GetManifestUrl();
                var json = await _http.GetStringAsync(url);
                var manifest = JsonSerializer.Deserialize<UpdateManifest>(json);

                if (manifest == null)
                    return;

                if (!IsNewer(manifest.Version))
                    return;

                var file = SelectFileForPlatform(manifest);
                if (file == null)
                    return;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    await UpdateWindowsAsync(file.Url);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    await UpdateLinuxAsync(file.Url);
            }
            catch
            {
                // Silent fail — app continues normally
            }
        }

        private static bool IsNewer(string remote)
        {
            try
            {
                var local = typeof(UpdateChecker).Assembly.GetName().Version?.ToString() ?? "0.0.0";
                return string.Compare(remote, local, StringComparison.OrdinalIgnoreCase) > 0;
            }
            catch
            {
                return false;
            }
        }

        private static UpdateFile? SelectFileForPlatform(UpdateManifest manifest)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return manifest.Files.FirstOrDefault(f => f.Type == "installer-exe");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return manifest.Files.FirstOrDefault(f => f.Type == "appimage");

            return null;
        }

        private static async Task UpdateWindowsAsync(string url)
        {
            try
            {
                var temp = Path.Combine(Path.GetTempPath(), "LumenRGB-Update.exe");
                var data = await _http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(temp, data);

                Process.Start(temp, "/S");
                Environment.Exit(0);
            }
            catch
            {
                // Silent fail
            }
        }

        private static async Task UpdateLinuxAsync(string url)
        {
            try
            {
                var exe = Environment.ProcessPath!;
                var dir = Path.GetDirectoryName(exe)!;
                var old = exe + ".old";
                var newFile = Path.Combine(dir, Path.GetFileName(exe));

                // Rename running AppImage
                File.Move(exe, old, true);

                // Download new AppImage
                var data = await _http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(newFile, data);

                // Make executable
                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList = { "+x", newFile }
                })?.WaitForExit();

                // Launch new version
                Process.Start(newFile);

                // Exit old version
                Environment.Exit(0);
            }
            catch
            {
                // Silent fail
            }
        }
    }

    public class UpdateManifest
    {
        public string Version { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public bool Prerelease { get; set; }
        public List<UpdateFile> Files { get; set; }
    }

    public class UpdateFile
    {
        public string Type { get; set; }
        public string Url { get; set; }
    }
}
