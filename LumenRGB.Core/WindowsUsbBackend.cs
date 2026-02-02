using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace LumenRGB.Core
{
    public static class WindowsUsbBackend
    {
        public static List<UsbSerialDevice> GetDevices()
        {
            var result = new List<UsbSerialDevice>();

            foreach (string hubPath in EnumerateUsbHubs())
            {
                using var hub = CreateFile(hubPath, 0, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
                if (hub.IsInvalid)
                    continue;

                for (uint port = 1; port <= 32; port++)
                {
                    var info = GetConnectionInfo(hub, port);
                    if (info == null || info.Value.DeviceIsHub != 0 || info.Value.ConnectionStatus == 0)
                        continue;

                    ushort vid = info.Value.idVendor;
                    ushort pid = info.Value.idProduct;

                    string manufacturer = info.Value.iManufacturer != 0 ? ReadString(hub, port, info.Value.iManufacturer) : null;
                    string product = info.Value.iProduct != 0 ? ReadString(hub, port, info.Value.iProduct) : null;
                    string serialNumber = info.Value.iSerialNumber != 0 ? ReadString(hub, port, info.Value.iSerialNumber) : null;

                    if (string.IsNullOrEmpty(serialNumber))
                        continue;

                    string comPort = ResolveComPort(vid, pid, serialNumber);
                    if (string.IsNullOrEmpty(comPort))
                        continue;

                    result.Add(new UsbSerialDevice
                    {
                        Port = comPort,
                        Vid = vid.ToString("X4"),
                        Pid = pid.ToString("X4"),
                        SerialNumber = serialNumber,
                        Manufacturer = manufacturer,
                        Product = product
                    });
                }
            }

            return result;
        }

        // ---------------- Hub enumeration ----------------

        private static Guid GUID_DEVINTERFACE_USB_HUB =
            new Guid("f18a0e88-c30c-11d0-8815-00a0c906bed8");

        private static IEnumerable<string> EnumerateUsbHubs()
        {
            IntPtr infoSet = SetupDiGetClassDevs(
                ref GUID_DEVINTERFACE_USB_HUB, null, IntPtr.Zero,
                (uint)(DIGCF.PRESENT | DIGCF.DEVICEINTERFACE));

            if (infoSet == IntPtr.Zero || infoSet.ToInt64() == -1)
                yield break;

            try
            {
                uint index = 0;
                var ifaceData = new SP_DEVICE_INTERFACE_DATA { cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>() };

                while (SetupDiEnumDeviceInterfaces(infoSet, IntPtr.Zero, ref GUID_DEVINTERFACE_USB_HUB, index, ref ifaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(infoSet, ref ifaceData, IntPtr.Zero, 0, out int required, IntPtr.Zero);

                    var detail = new SP_DEVICE_INTERFACE_DETAIL_DATA
                    {
                        cbSize = IntPtr.Size == 8 ? 8 : 4 + Marshal.SystemDefaultCharSize
                    };

                    if (SetupDiGetDeviceInterfaceDetail(infoSet, ref ifaceData, ref detail, required, out _, IntPtr.Zero))
                        yield return detail.DevicePath;

                    index++;
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(infoSet);
            }
        }

        private static USB_NODE_CONNECTION_INFORMATION_EX? GetConnectionInfo(SafeFileHandle hub, uint port)
        {
            int size = Marshal.SizeOf<USB_NODE_CONNECTION_INFORMATION_EX>();
            IntPtr buffer = Marshal.AllocHGlobal(size);

            try
            {
                var info = new USB_NODE_CONNECTION_INFORMATION_EX { ConnectionIndex = port };
                Marshal.StructureToPtr(info, buffer, false);

                if (!DeviceIoControl(hub, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX,
                    buffer, size, buffer, size, out _, IntPtr.Zero))
                    return null;

                return Marshal.PtrToStructure<USB_NODE_CONNECTION_INFORMATION_EX>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string ReadString(SafeFileHandle hub, uint port, byte index)
        {
            int size = Marshal.SizeOf<USB_DESCRIPTOR_REQUEST>() + 256;
            IntPtr buffer = Marshal.AllocHGlobal(size);

            try
            {
                var req = new USB_DESCRIPTOR_REQUEST
                {
                    ConnectionIndex = port,
                    SetupPacket = new USB_SETUP_PACKET
                    {
                        bmRequest = 0x80,
                        bRequest = 6,
                        wValue = (ushort)((USB_STRING_DESCRIPTOR_TYPE << 8) | index),
                        wIndex = 0x0409,
                        wLength = 255
                    }
                };

                Marshal.StructureToPtr(req, buffer, false);

                if (!DeviceIoControl(hub, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION,
                    buffer, size, buffer, size, out int returned, IntPtr.Zero))
                    return null;

                if (returned < Marshal.SizeOf<USB_DESCRIPTOR_REQUEST>() + 2)
                    return null;

                IntPtr descPtr = buffer + Marshal.SizeOf<USB_DESCRIPTOR_REQUEST>();
                var desc = Marshal.PtrToStructure<USB_STRING_DESCRIPTOR>(descPtr);

                if (desc.bLength < 2)
                    return null;

                int len = desc.bLength - 2;
                return Encoding.Unicode.GetString(desc.bString, 0, len).TrimEnd('\0');
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        // ---------------- COM port resolver ----------------

        public static string ResolveComPort(ushort vid, ushort pid, string serial)
        {
            string instanceId = $"USB\\VID_{vid:X4}&PID_{pid:X4}\\{serial}";

            if (CM_Locate_DevNode(out uint devInst, instanceId, 0) != CR.SUCCESS)
                return null;

            return FindComRecursive(devInst);
        }

        private static string FindComRecursive(uint devInst)
        {
            if (TryExtractComFromFriendly(GetFriendlyNameByInstanceId(GetDeviceIdFromDevInst(devInst)), out string com))
                return com;

            if (CM_Get_Child(out uint child, devInst, 0) == CR.SUCCESS)
            {
                string found = FindComRecursive(child);
                if (found != null) return found;
            }

            if (CM_Get_Sibling(out uint sibling, devInst, 0) == CR.SUCCESS)
            {
                string found = FindComRecursive(sibling);
                if (found != null) return found;
            }

            return null;
        }

        private static bool TryExtractComFromFriendly(string friendly, out string comPort)
        {
            comPort = null;
            if (string.IsNullOrEmpty(friendly))
                return false;

            int p1 = friendly.IndexOf("(COM", StringComparison.OrdinalIgnoreCase);
            if (p1 < 0) return false;

            int p2 = friendly.IndexOf(")", p1);
            if (p2 <= p1) return false;

            comPort = friendly.Substring(p1 + 1, p2 - p1 - 1);
            return true;
        }

        private static string GetDeviceIdFromDevInst(uint devInst)
        {
            var sb = new StringBuilder(MAX_DEVICE_ID_LEN);
            return CM_Get_Device_ID(devInst, sb, sb.Capacity, 0) == CR.SUCCESS ? sb.ToString() : null;
        }

        private static string GetFriendlyNameByInstanceId(string instanceId)
        {
            if (instanceId == null)
                return null;

            instanceId = instanceId.ToUpperInvariant();

            IntPtr infoSet = SetupDiGetClassDevs(IntPtr.Zero, null, IntPtr.Zero,
                DIGCF.ALLCLASSES | DIGCF.PRESENT);

            if (infoSet == IntPtr.Zero || infoSet.ToInt64() == -1)
                return null;

            try
            {
                uint index = 0;
                var devInfo = new SP_DEVINFO_DATA { cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>() };

                while (SetupDiEnumDeviceInfo(infoSet, index, ref devInfo))
                {
                    index++;

                    string id = GetDeviceIdFromDevInst(devInfo.DevInst);
                    if (id == null || !id.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string friendly = GetDevicePropertyString(infoSet, ref devInfo, SPDRP.SPDRP_FRIENDLYNAME);
                    return string.IsNullOrEmpty(friendly)
                        ? GetDevicePropertyString(infoSet, ref devInfo, SPDRP.SPDRP_DEVICEDESC)
                        : friendly;
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(infoSet);
            }

            return null;
        }

        private static string GetDevicePropertyString(
            IntPtr infoSet, ref SP_DEVINFO_DATA devInfo, SPDRP property)
        {
            byte[] buffer = new byte[512];

            bool ok = SetupDiGetDeviceRegistryProperty(
                infoSet, ref devInfo, property, out _, buffer,
                (uint)buffer.Length, out uint required);

            return ok && required > 0
                ? Encoding.Unicode.GetString(buffer, 0, (int)required).TrimEnd('\0')
                : null;
        }

        // ---------------- P/Invoke + structs ----------------

        private const uint IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX = 0x220448;
        private const uint IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = 0x220410;
        private const byte USB_STRING_DESCRIPTOR_TYPE = 0x03;
        private const int MAX_DEVICE_ID_LEN = 200;

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_SETUP_PACKET
        {
            public byte bmRequest, bRequest;
            public ushort wValue, wIndex, wLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_DESCRIPTOR_REQUEST
        {
            public uint ConnectionIndex;
            public USB_SETUP_PACKET SetupPacket;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_STRING_DESCRIPTOR
        {
            public byte bLength, bDescriptorType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
            public byte[] bString;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USB_NODE_CONNECTION_INFORMATION_EX
        {
            public uint ConnectionIndex;
            public byte bLength, bDescriptorType;
            public ushort bcdUSB;
            public byte bDeviceClass, bDeviceSubClass, bDeviceProtocol, bMaxPacketSize0;
            public ushort idVendor, idProduct, bcdDevice;
            public byte iManufacturer, iProduct, iSerialNumber, bNumConfigurations;
            public uint ConnectionStatus;
            public byte CurrentConfigurationValue, Speed, DeviceIsHub;
            public ushort DeviceAddress;
            public uint NumberOfOpenPipes, ConnectionInformationFlags;
            public IntPtr PipeList;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        private enum CR : uint { SUCCESS = 0x00000000 }

        private enum SPDRP : uint
        {
            SPDRP_DEVICEDESC = 0x00000000,
            SPDRP_FRIENDLYNAME = 0x0000000C
        }

        [Flags]
        private enum DIGCF : uint
        {
            DEFAULT = 0x00000001,
            PRESENT = 0x00000002,
            ALLCLASSES = 0x00000004,
            PROFILE = 0x00000008,
            DEVICEINTERFACE = 0x00000010
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string fileName, uint desiredAccess, uint shareMode,
            IntPtr securityAttributes, uint creationDisposition,
            uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeFileHandle deviceHandle, uint ioControlCode,
            IntPtr inBuffer, int inBufferSize,
            IntPtr outBuffer, int outBufferSize,
            out int bytesReturned, IntPtr overlapped);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid, string Enumerator,
            IntPtr hwndParent, uint Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet, IntPtr DeviceInfoData,
            ref Guid InterfaceClassGuid, uint MemberIndex,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize,
            out int RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize, out int RequiredSize,
            IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern CR CM_Locate_DevNode(out uint pdnDevInst, string pDeviceID, uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        private static extern CR CM_Get_Child(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

        [DllImport("cfgmgr32.dll")]
        private static extern CR CM_Get_Sibling(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern CR CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, uint ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
            IntPtr ClassGuid, string Enumerator,
            IntPtr hwndParent, DIGCF Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet, uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData,
            SPDRP Property, out uint PropertyRegDataType,
            byte[] PropertyBuffer, uint PropertyBufferSize,
            out uint RequiredSize);
    }
}