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
    public static class UpdateChecker
    {
        private static readonly HttpClient _http = new();

        static UpdateChecker()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("LumenRGB-Updater/1.0");
        }

        private const string ManifestUrl =
            "https://github.com/Connors-Studios/LumenRGB/releases/latest/download/update.json";

        public static async Task CheckForUpdatesAsync()
        {
            try
            {
                var json = await _http.GetStringAsync(ManifestUrl);

                var manifest = JsonSerializer.Deserialize<UpdateManifest>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

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
                // Silent fail
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

                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = temp,
                    Arguments = "/S /L",
                    UseShellExecute = true
                });

                // Exit current application
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

                File.Move(exe, old, true);

                var data = await _http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(newFile, data);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList = { "+x", newFile }
                })?.WaitForExit();

                Process.Start(newFile);

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
        public List<UpdateFile> Files { get; set; }
    }

    public class UpdateFile
    {
        public string Type { get; set; }
        public string Url { get; set; }
    }
}
