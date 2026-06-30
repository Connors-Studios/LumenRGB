using System;
using System.Collections.Generic;
using System.Linq;

namespace LumenRGB.Core
{
    public class LumenDiscoverer
    {
        // These values are used to identify Lumen devices.
        // NOTE: 303A:4002 is the default Espressif/TinyUSB ESP32-S3 VID/PID and
        // is shared by many other boards, so it alone is NOT enough to confirm a
        // LumenRGB device. The firmware also sets its USB serial descriptor to
        // "LRGB-<MAC>" (e.g. LRGB-10B41DD29CD4), which we additionally require.
        const string LUMEN_VID    = "303A"; // Espressif VID (NOT REGISTERED, WILL BE CHANGED)
        const string LUMEN_PID    = "4002"; // Espressif PID (NOT REGISTERED, WILL BE CHANGED)
        const string SERIAL_PREFIX = "LRGB-";

        public static List<UsbSerialDevice> GetLumenDevices()
        {
            return UsbSerialEnumerator.GetComDevices()
                .Where(IsLumenDevice)
                .ToList();
        }

        /// <summary>
        /// True if a USB device looks like a LumenRGB board: matching Espressif
        /// VID/PID and a serial descriptor in the firmware's "LRGB-..." format.
        /// </summary>
        public static bool IsLumenDevice(UsbSerialDevice device)
        {
            return string.Equals(device.Vid, LUMEN_VID, StringComparison.OrdinalIgnoreCase)
                && string.Equals(device.Pid, LUMEN_PID, StringComparison.OrdinalIgnoreCase)
                && device.SerialNumber != null
                && device.SerialNumber.StartsWith(SERIAL_PREFIX, StringComparison.OrdinalIgnoreCase);
        }
    }
}
