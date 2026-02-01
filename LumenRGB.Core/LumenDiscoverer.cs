using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumenRGB.Core
{
    public class LumenDiscoverer
    {
        // These values are used to identify Lumen devices
        string LUMEN_VID = "303A"; // Espressif VID (NOT REGISTERED and WILL BE CHANGED)
        string LUMEN_PID = "4002"; // Espressif PID (NOT REGISTERED and WILL BE CHANGED)
        
        public void DiscoverLumenDevices()
        {
            var devices = GetDevices();
            var lumenDevices = devices
                .Where(d => string.Equals(d.Pid, LUMEN_PID, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(d.Vid, LUMEN_VID, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<UsbSerialDevice> GetDevices()
        {
            var devices = UsbSerialEnumerator.GetComDevices();
            return devices;
        }
    }
}
