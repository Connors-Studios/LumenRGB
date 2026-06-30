using LumenRGB.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LumenRGB.Core
{
    public class UsbSerialDevice
    {
        // Port is always present (a COM port on Windows, a /dev node on Linux).
        // The remaining fields come from optional USB descriptors / sysfs reads
        // and may be absent, so they are nullable.
        public required string Port { get; set; }
        public string? SerialNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? Product { get; set; }
        public string? Vid { get; set; }
        public string? Pid { get; set; }
    }

    public static class UsbSerialEnumerator
    {
        public static List<UsbSerialDevice> GetComDevices()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsUsbBackend.GetDevices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return LinuxUsbBackend.GetDevices();

            throw new PlatformNotSupportedException("Only Windows and Linux are supported.");
        }
    }
}