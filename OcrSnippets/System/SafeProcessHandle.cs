using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo
{
	//from reflector
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		// Fields
		internal static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero);

		// Methods
		internal SafeProcessHandle()
			: base(true)
		{
		}

		internal SafeProcessHandle(IntPtr handle)
			: base(true)
		{
			base.SetHandle(handle);
		}

		internal void InitialSetHandle(IntPtr h)
		{
			base.handle = h;
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		private static extern bool CloseHandle(IntPtr handle);

		protected override bool ReleaseHandle()
		{
			return CloseHandle(base.handle);
		}
	}

}
