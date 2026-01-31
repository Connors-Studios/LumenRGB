using LumenRGB.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LumenRGB.Core
{
    public class UsbSerialDevice
    {
        public string Port { get; set; }
        public string Serial { get; set; }
        public string Manufacturer { get; set; }
        public string Product { get; set; }
        public string Vid { get; set; }
        public string Pid { get; set; }
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