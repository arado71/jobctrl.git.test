namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class ProcessNameHelper
    {
        private static readonly bool isVistaOrLater = ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 6));
        private static readonly int[] sizes = new int[] { 260, 0x400, 0x7fff };

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool QueryFullProcessImageName(SafeProcessHandle hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);
        public static bool TryGetProcessName(int processId, out string processName)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    processName = process.MainModule.ModuleName;
                    return true;
                }
            }
            catch (Exception)
            {
                if (isVistaOrLater)
                {
                    using (SafeProcessHandle handle = SafeProcessHandle.OpenProcess(0x1000, false, processId))
                    {
                        if (!handle.IsInvalid)
                        {
                            return TryQueryProcessFileName(handle, out processName);
                        }
                        Marshal.GetLastWin32Error();
                    }
                }
            }
            processName = null;
            return false;
        }

        private static bool TryQueryFullProcessImageName(SafeProcessHandle handle, out string path)
        {
            foreach (int num in sizes)
            {
                int capacity = num;
                StringBuilder lpExeName = new StringBuilder(capacity);
                if (!QueryFullProcessImageName(handle, 0, lpExeName, ref capacity))
                {
                    if (Marshal.GetLastWin32Error() != 0x7a)
                    {
                        break;
                    }
                }
                else
                {
                    path = lpExeName.ToString();
                    return true;
                }
            }
            path = null;
            return false;
        }

        private static bool TryQueryProcessFileName(SafeProcessHandle handle, out string fileName)
        {
            try
            {
                string str;
                if (TryQueryFullProcessImageName(handle, out str))
                {
                    fileName = Path.GetFileName(str);
                    return true;
                }
            }
            catch (Exception)
            {
            }
            fileName = null;
            return false;
        }

        private enum ProcessAccessFlags
        {
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000
        }
    }
}

