namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero);

        internal SafeProcessHandle() : base(true)
        {
        }

        internal SafeProcessHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern bool CloseHandle(IntPtr handle);
        internal void InitialSetHandle(IntPtr h)
        {
            base.handle = h;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);
        protected override bool ReleaseHandle()
	    {
		    return CloseHandle(base.handle);
	    }
    }
}

