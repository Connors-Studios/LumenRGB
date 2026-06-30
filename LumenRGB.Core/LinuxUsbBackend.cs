using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LumenRGB.Core
{

    public static class LinuxUsbBackend
    {
        public static List<UsbSerialDevice> GetDevices()
        {
            return Directory.GetFiles("/dev")
                .Where(f => f.StartsWith("/dev/ttyACM") || f.StartsWith("/dev/ttyUSB"))
                .Select(CreateDevice)
                .OfType<UsbSerialDevice>()
                .ToList();
        }

        private static UsbSerialDevice? CreateDevice(string port)
        {
            string sys = $"/sys/class/tty/{Path.GetFileName(port)}/device";
            if (!Directory.Exists(sys))
                return null;

            string usb = Path.GetFullPath(Path.Combine(sys, "../.."));

            return new UsbSerialDevice
            {
                Port = port,
                Vid = Read(usb + "/idVendor"),
                Pid = Read(usb + "/idProduct"),
                SerialNumber = Read(usb + "/serial"),
                Manufacturer = Read(usb + "/manufacturer"),
                Product = Read(usb + "/product")
            };
        }

        private static string? Read(string path) =>
            File.Exists(path) ? File.ReadAllText(path).Trim() : null;
    }
}