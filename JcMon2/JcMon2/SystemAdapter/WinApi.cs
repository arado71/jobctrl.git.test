using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace JcMon2.SystemAdapter
{
	public class WinApi
	{
		public const int WM_HOTKEY = 0x0312;

		[Flags]
		public enum WS_EX
		{
			TOPMOST = 0x00000008,
		}

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		// Unregisters the hot key with Windows.
		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(
			[In] IntPtr hObject);
	}
}
