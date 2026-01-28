using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace LumenRGB.UI.Avalonia.Desktop
{
    public static class UpdateChecker
    {
        private static readonly HttpClient _http = new();

        static UpdateChecker()
        {
            // Add a User-Agent header to avoid being blocked by some servers (e.g., GitHub)
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("LumenRGB-Updater/1.1");
        }

        // URL to the update manifest JSON file
        private const string ManifestUrl = "https://github.com/Connors-Studios/LumenRGB/releases/latest/download/update.json";

        // Check for updates asynchronously
        public static async Task CheckForUpdatesAsync()
        {
            try
            {
                // Fetch the update manifest
                var json = await _http.GetStringAsync(ManifestUrl);

                // Deserialize the manifest
                var manifest = JsonSerializer.Deserialize<UpdateManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // If manifest is null, exit
                if (manifest == null)
                    return;

                // Compare versions to see if an update is needed
                if (!IsNewer(manifest.Version))
                    return;

                // Select the correct file for the current platform
                var file = SelectFileForPlatform(manifest);
                if (file == null)
                    return;

                // Perform the update based on the platform
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

        // Compare version strings to determine if the remote version is newer
        private static bool IsNewer(string remote)
        {
            try
            {
                // Get the local version from the assembly
                var local = typeof(UpdateChecker).Assembly.GetName().Version?.ToString() ?? "0.0.0";
                return string.Compare(remote, local, StringComparison.OrdinalIgnoreCase) > 0;
            }
            catch
            {
                // In case of error, assume not newer
                return false;
            }
        }

        // Select the correct update file for the current platform
        private static UpdateFile? SelectFileForPlatform(UpdateManifest manifest)
        {
            // Windows: installer-exe
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return manifest.Files.FirstOrDefault(f => f.Type == "installer-exe");

            // Linux: appimage
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return manifest.Files.FirstOrDefault(f => f.Type == "appimage");

            // Unsupported platform
            return null;
        }

        // Update process for Windows
        private static async Task UpdateWindowsAsync(string url)
        {
            try
            {
                // Download the installer to a temporary location
                var temp = Path.Combine(Path.GetTempPath(), "LumenRGB-Update.exe");

                // Download the installer
                var data = await _http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(temp, data);

                // Start the installer with silent and relaunch arguments
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

        // Update process for Linux
        private static async Task UpdateLinuxAsync(string url)
        {
            try
            {
                // Get the current executable and add .old suffix 
                var exe = Environment.ProcessPath!;
                var dir = Path.GetDirectoryName(exe)!;
                var old = exe + ".old";
                var newFile = Path.Combine(dir, Path.GetFileName(exe));

                // Move current executable to .old
                File.Move(exe, old, true);

                // Download the new AppImage
                var data = await _http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(newFile, data);

                // Make the new file executable
                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList = { "+x", newFile }
                })?.WaitForExit();

                // Start the new executable
                Process.Start(newFile);

                // Exit current application
                Environment.Exit(0);
            }
            catch
            {
                // Silent fail
            }
        }
    }

    // Update manifest structure
    public class UpdateManifest
    {
        public required string Version { get; set; }
        public required string Name { get; set; }
        public required string Notes { get; set; }
        public required List<UpdateFile> Files { get; set; }
    }

    // Update file structure
    public class UpdateFile
    {
        public required string Type { get; set; }
        public required string Url { get; set; }
    }
}
