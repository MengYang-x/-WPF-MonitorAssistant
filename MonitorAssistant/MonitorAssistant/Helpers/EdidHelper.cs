using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MonitorAssistant.Helpers
{
    public static class EdidHelper
    {
        // Windows API 声明
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        private const int ENUM_CURRENT_SETTINGS = -1;

        // 结构体定义
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            // 其他字段省略...
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFrequency;
            // 其他字段省略...
        }

        public static List<MonitorInfo> GetMonitorEdidInfo()
        {
            var monitors = new List<MonitorInfo>();

            try
            {
                // 方法1: 使用WMI获取基本信息
                GetMonitorInfoViaWMI(monitors);

                // 方法2: 使用Windows API获取补充信息
                GetMonitorInfoViaAPI(monitors);

                if (monitors.Count == 0)
                {
                    Debug.WriteLine("警告：未检测到任何显示器信息");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取EDID信息时发生错误：{ex}");
                throw; // 将异常传递给调用者
            }

            return monitors;
        }

        private static void GetMonitorInfoViaWMI(List<MonitorInfo> monitors)
        {
            try
            {
                var scope = new ManagementScope("\\\\.\\root\\wmi");
                var query = new ObjectQuery("SELECT * FROM WmiMonitorID");

                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    searcher.Options.Timeout = TimeSpan.FromSeconds(10);

                    foreach (ManagementObject mo in searcher.Get())
                    {
                        var monitor = new MonitorInfo();

                        // 获取制造商信息
                        monitor.Manufacturer = GetWmiPropertyString(mo, "ManufacturerName");

                        // 获取产品代码
                        monitor.ProductCode = GetWmiPropertyString(mo, "ProductCodeID");

                        // 获取序列号
                        monitor.SerialNumber = GetWmiPropertyString(mo, "SerialNumberID");

                        // 获取友好名称
                        monitor.FriendlyName = GetWmiPropertyString(mo, "UserFriendlyName");

                        monitors.Add(monitor);
                    }
                }
            }
            catch (ManagementException mex)
            {
                Debug.WriteLine($"WMI查询失败：{mex.Message}");
                if (mex.ErrorCode == ManagementStatus.AccessDenied)
                {
                    throw new UnauthorizedAccessException("需要管理员权限访问WMI信息", mex);
                }
            }
        }

        private static string GetWmiPropertyString(ManagementObject mo, string propertyName)
        {
            try
            {
                if (mo[propertyName] != null)
                {
                    byte[] bytes = (byte[])mo[propertyName];
                    return ParseEdidString(bytes);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"读取WMI属性{propertyName}失败：{ex.Message}");
            }
            return string.Empty;
        }

        private static void GetMonitorInfoViaAPI(List<MonitorInfo> monitors)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            uint deviceIndex = 0;

            while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
            {
                if ((d.StateFlags & 0x1) != 0) // DISPLAY_DEVICE_ACTIVE
                {
                    DEVMODE devMode = new DEVMODE();
                    if (EnumDisplaySettings(d.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode) != 0)
                    {
                        // 尝试匹配WMI获取的显示器
                        var monitor = monitors.Find(m =>
                            !string.IsNullOrEmpty(m.ProductCode) &&
                            d.DeviceID.Contains(m.ProductCode)) ?? new MonitorInfo();

                        // 补充API获取的信息
                        monitor.DeviceName = d.DeviceName;
                        monitor.DeviceString = d.DeviceString;
                        monitor.DeviceID = d.DeviceID;
                        monitor.Resolution = $"{devMode.dmPelsWidth}x{devMode.dmPelsHeight}";
                        monitor.RefreshRate = devMode.dmDisplayFrequency;

                        if (!monitors.Contains(monitor))
                        {
                            monitors.Add(monitor);
                        }
                    }
                }
                d.cb = Marshal.SizeOf(d);
                deviceIndex++;
            }
        }

        private static string ParseEdidString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            int length = Array.IndexOf(bytes, (byte)0x0A);
            if (length < 0) length = bytes.Length;

            return System.Text.Encoding.ASCII.GetString(bytes, 0, length).Trim();
        }

        // 重启WMI服务的辅助方法
        public static void RestartWMIService()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "net.exe",
                        Arguments = "stop winmgmt /y",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas" // 以管理员身份运行
                    };
                    process.Start();
                    process.WaitForExit(5000);

                    process.StartInfo.Arguments = "start winmgmt";
                    process.Start();
                    process.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"重启WMI服务失败：{ex.Message}");
            }
        }
    }

    public class MonitorInfo
    {
        public string DeviceName { get; set; } = "未知设备";
        public string DeviceString { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = "未知显示器";
        public string Resolution { get; set; } = string.Empty;
        public int RefreshRate { get; set; }

        public override string ToString()
        {
            return $"{FriendlyName} ({Manufacturer}) {Resolution}@{RefreshRate}Hz";
        }
    }
}