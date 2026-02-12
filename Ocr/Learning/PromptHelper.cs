using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using log4net;

namespace TesseractFontTeacher
{
	public static class PromptHelper
	{
		private const int STARTF_USESHOWWINDOW = 1;
		private const int SW_SHOWNOACTIVATE = 4;
		private const int SW_SHOWMINNOACTIVE = 7;
		private const uint INFINITE = 0xFFFFFFFF;
		private const uint CREATE_NO_WINDOW = 0x08000000;
		private const uint NORMAL_PRIORITY_CLASS = 0x00000020;
		private static readonly ILog log = LogManager.GetLogger(typeof(PromptHelper));

		public static int Run(string command, string parameters, string workingDir = null, int timeOut = 10000)
		{
			log.DebugFormat("Running: \"{0}\" {1} inside {2}", command, parameters, workingDir);
			return (int) StartProcess(command + " " + parameters, workingDir, (uint) timeOut);
			var pi = new ProcessStartInfo(command, parameters) { CreateNoWindow = true, UseShellExecute = false };
			if (!string.IsNullOrEmpty(workingDir))
				pi.WorkingDirectory = workingDir;

			var process = Process.Start(pi);
			var res = process.WaitForExit(timeOut);
			return res ? process.ExitCode : -1;
		}

		[DllImport("kernel32.dll")]
		private static extern bool CreateProcess(
			string lpApplicationName,
			string lpCommandLine,
			IntPtr lpProcessAttributes,
			IntPtr lpThreadAttributes,
			bool bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			[In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation
		);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

		private static uint StartProcess(string cmdLine, string workingDir, uint timeOut = 5000)
		{
			var si = new STARTUPINFO();
			si.cb = Marshal.SizeOf(si);
			si.dwFlags = STARTF_USESHOWWINDOW;
			si.wShowWindow = SW_SHOWMINNOACTIVE;

			var pi = new PROCESS_INFORMATION();

			CreateProcess(null, cmdLine, IntPtr.Zero, IntPtr.Zero, true,
				CREATE_NO_WINDOW | NORMAL_PRIORITY_CLASS, IntPtr.Zero, workingDir, ref si, out pi);

			WaitForSingleObject(pi.hProcess, timeOut);
			uint res;
			GetExitCodeProcess(pi.hProcess, out res);
			CloseHandle(pi.hProcess);
			CloseHandle(pi.hThread);

			return res;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct STARTUPINFO
		{
			public int cb;
			public readonly string lpReserved;
			public readonly string lpDesktop;
			public readonly string lpTitle;
			public readonly int dwX;
			public readonly int dwY;
			public readonly int dwXSize;
			public readonly int dwYSize;
			public readonly int dwXCountChars;
			public readonly int dwYCountChars;
			public readonly int dwFillAttribute;
			public int dwFlags;
			public short wShowWindow;
			public readonly short cbReserved2;
			public readonly IntPtr lpReserved2;
			public readonly IntPtr hStdInput;
			public readonly IntPtr hStdOutput;
			public readonly IntPtr hStdError;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}
	}
}