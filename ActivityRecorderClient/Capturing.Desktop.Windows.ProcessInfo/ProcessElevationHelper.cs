using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using log4net;
using Microsoft.Win32.SafeHandles;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo
{
	//http://www.itwriting.com/blog/198-c-code-to-detect-uac-elevation-on-vista.html
	//http://blogs.msdn.com/b/msdnforum/archive/2010/03/30/a-quick-start-guide-of-process-mandatory-level-checking-and-self-elevation-under-uac.aspx
	public static class ProcessElevationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly bool isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;

		public static bool IsElevated()
		{
			if (!isVistaOrLater) return true;
			return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		}

		public static bool IsElevated(int pid)
		{
			if (!isVistaOrLater) return true;
			using (var safeHandle = SafeProcessHandle.OpenProcess((int)ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, pid))
			{
				if (safeHandle.IsInvalid)
				{
					throw new Exception("Cannot open process", new Win32Exception(Marshal.GetLastWin32Error()));
				}
				var tokenHandle = IntPtr.Zero;
				var tokenElevationPtr = IntPtr.Zero;
				try
				{
					if (!OpenProcessToken(safeHandle, TOKEN_QUERY, out tokenHandle))
					{
						throw new Exception("Cannot open process token", new Win32Exception(Marshal.GetLastWin32Error()));
					}

					var tokenInfLength = Marshal.SizeOf(typeof(TokenElevation));
					tokenElevationPtr = Marshal.AllocHGlobal(tokenInfLength);
					if (!GetTokenInformation(tokenHandle, TokenInformationClass.TokenElevation, tokenElevationPtr, tokenInfLength, out tokenInfLength))
					{
						throw new Exception("Cannot get token info", new Win32Exception(Marshal.GetLastWin32Error()));
					}

					var elevation = (TokenElevation)Marshal.PtrToStructure(tokenElevationPtr, typeof(TokenElevation));
					return elevation.TokenIsElevated != 0;
				}
				finally
				{
					if (tokenElevationPtr != IntPtr.Zero) Marshal.FreeHGlobal(tokenElevationPtr);
					if (tokenHandle != IntPtr.Zero) CloseHandle(tokenHandle);
				}
			}
		}

		public static bool Elevate()
		{
			try
			{
				var startInfo = GetElevatedProcessStartInfo(Assembly.GetEntryAssembly().Location, Environment.GetCommandLineArgs().Skip(1).ToArray());
				Process.Start(startInfo);
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Unable to elevate process", ex);
				return false;
			}
		}

		public static ProcessStartInfo GetElevatedProcessStartInfo(string fileName, params string[] args)
		{
			return new ProcessStartInfo(fileName)
			{
				Arguments = string.Join(" ", args),
				Verb = "runas",
			};
		}

		public static bool Unelevate()
		{
			try
			{
				StartProcessUnelevated(Assembly.GetEntryAssembly().Location, Environment.GetCommandLineArgs().Skip(1).ToArray());
				return true;
			}
			catch (Exception ex)
			{
				log.Warn($"Unable to unelevate process via token change, trying vbs method... ({ex})");
				try
				{
					var startInfo = GetUnelevatedProcessStartInfo(Assembly.GetEntryAssembly().Location, Environment.GetCommandLineArgs().Skip(1).ToArray());
					Process.Start(startInfo);
					return true;
				}
				catch (Exception ex2)
				{
					log.Error("Unable to unelevate process", ex2);
					return false;
				}
			}
		}

		public static void StartProcessUnelevated(string fileName, params string[] args)
		{
			var currentSessionID = Process.GetCurrentProcess().SessionId;
			var processes = Process.GetProcessesByName("explorer");
			var hUserTokenDup = IntPtr.Zero;
			try
			{
				log.Debug("base processes: " + string.Join(", ", processes.Where(p => p.SessionId == currentSessionID).Select(p => p.Id)));
				var baseProcess = processes.FirstOrDefault(p => p.SessionId == currentSessionID);
				if (baseProcess == null) throw new Exception("no explorer process");
				if (!DuplicateToken((uint)baseProcess.Id, out var secAttrs, ref hUserTokenDup))
					throw new Exception("Duplicate token failed, process can't start unelevated");
				var procInfo = new PROCESS_INFORMATION();
				if (!CreateProcessAsUser(fileName, args, hUserTokenDup, secAttrs, ref procInfo))
					throw new Exception("Create process as user failed, process can't start unelevated");
			}
			finally
			{
				if (hUserTokenDup != IntPtr.Zero) CloseHandle(hUserTokenDup);
				foreach (var process in processes)
				{
					process?.Dispose();
				}
			}
		}

		//This method has a side effect of creating a temp file.
		public static ProcessStartInfo GetUnelevatedProcessStartInfo(string fileName, params string[] args)
		{
			var arguments = "\"" + fileName + "\"";
			var sb = new StringBuilder();
			foreach (var arg in args)
			{
				sb.Append(" \"").Append(arg).Append("\"");
			}
			if (sb.Length != 0)
			{
				var tempFolder = Path.Combine(Path.GetTempPath(), "JobCTRL");
				if (!Directory.Exists(tempFolder))
				{
					Directory.CreateDirectory(tempFolder);
				}

				var tempFile = Path.Combine(tempFolder, Path.GetRandomFileName());
				File.WriteAllText(tempFile, string.Format(vbsExecFormat, arguments.Replace("\"", "\"\"") + sb.Replace("\"", "\"\"")), Encoding.Default);
				File.Move(tempFile, tempFile + ".vbs");
				arguments = "\"" + tempFile + ".vbs\"";
			}
			return new ProcessStartInfo("explorer.exe")
			{
				Arguments = arguments,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = Environment.CurrentDirectory,
				CreateNoWindow = true,  //TODO: run vbs file is popping up a console window
			};
		}

		private const string vbsExecFormat =
				"Dim WshShell, oExec\n" +
				"Set WshShell = CreateObject(\"WScript.Shell\")\n" +
				"Set oExec = WshShell.Exec(\"{0}\")\n" +
				//self destruct script
				"Set objFSO = CreateObject(\"Scripting.FileSystemObject\")\n" +
				"strScript = Wscript.ScriptFullName\n" +
				"objFSO.DeleteFile(strScript)\n"
			;

		public static bool CreateProcessAsUser(string applicationName, string[] args, IntPtr hUserTokenDup, SECURITY_ATTRIBUTES sa,
			ref PROCESS_INFORMATION procInfo)
		{
			// By default CreateProcessAsUser creates a process on a non-interactive window station, meaning
			// the window station has a desktop that is invisible and the process is incapable of receiving
			// user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
			// interaction with the new process.
			STARTUPINFO si = new STARTUPINFO();
			si.cb = (int)Marshal.SizeOf(si);
			si.lpDesktop = @"winsta0\default";
			// interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop

			// flags that specify the priority and creation method of the process
			int dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE | CREATE_UNICODE_ENVIRONMENT;

			// create a new process in the current user's logon session
			IntPtr envBlk;
			if (!CreateEnvironmentBlock(out envBlk, hUserTokenDup, false))
			{
				log.Debug("Can't create env block");
				return false;
			}

			var luid = default(LUID);
			if (!LookupPrivilegeValue(null, SE_ASSIGNPRIMARYTOKEN_NAME, ref luid))
			{
				var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
				log.Debug("LookupPrivilegeValue failed, lastError: " + errorMessage);
				return false;
			}
			TOKEN_PRIVILEGES newState = new TOKEN_PRIVILEGES() { PrivilegeCount = 1, Privileges = new []{ new LUID_AND_ATTRIBUTES() { Luid = luid, Attributes = SE_PRIVILEGE_ENABLED } }};
			TOKEN_PRIVILEGES prevState = default(TOKEN_PRIVILEGES);
			var sz = Marshal.SizeOf(newState);
			if (!AdjustTokenPrivileges(hUserTokenDup, false, ref newState, sz, ref prevState, out var __))
			{
				var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
				log.Debug("AdjustTokenPrivileges failed, lastError: " + errorMessage);
				return false;
			}
			var lpCurrentDirectory = Path.GetDirectoryName(applicationName);
			bool result = CreateProcessAsUser(hUserTokenDup, // client's access token
				args != null ? null : applicationName, // file to execute
				args != null ? "\"" + applicationName + "\" \"" + string.Join("\" \"", args) + "\"" : null, // command line
				ref sa, // pointer to process SECURITY_ATTRIBUTES
				ref sa, // pointer to thread SECURITY_ATTRIBUTES
				false, // handles are not inheritable
				dwCreationFlags, // creation flags
				envBlk, // pointer to new environment block 
				lpCurrentDirectory, // name of current directory 
				ref si, // pointer to STARTUPINFO structure
				out procInfo // receives information about new process
				);
			// should be called DestroyEnvironmentBlock
			if (!result)
			{
				var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
				log.Debug("CreateProcessAsUser failed, lastError: " + errorMessage);
			}

			return result;
		}

		public static bool DuplicateToken(uint sourcePid, out SECURITY_ATTRIBUTES sa, ref IntPtr hUserTokenDup)
		{
			IntPtr hPToken = IntPtr.Zero;
			sa = new SECURITY_ATTRIBUTES();
			// obtain a handle to the winlogon process
			var hProcess = OpenProcess(MAXIMUM_ALLOWED, false, sourcePid);

			// obtain a handle to the access token of the winlogon process
			if (!OpenProcessToken(hProcess, TOKEN_DUPLICATE, ref hPToken))
			{
				CloseHandle(hProcess);
				log.Debug("No process token for duplicate, pid: " + sourcePid);
				return false;
			}

			// Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser
			// I would prefer to not have to use a security attribute variable and to just 
			// simply pass null and inherit (by default) the security attributes
			// of the existing token. However, in C# structures are value types and therefore
			// cannot be assigned the null value.
			sa = new SECURITY_ATTRIBUTES();
			sa.Length = Marshal.SizeOf(sa);

			// copy the access token of the winlogon process; the newly created token will be a primary token
			if (
				!DuplicateTokenEx(hPToken, MAXIMUM_ALLOWED, ref sa, (int)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
					(int)TOKEN_TYPE.TokenPrimary, ref hUserTokenDup))
			{
				CloseHandle(hProcess);
				CloseHandle(hPToken);
				var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
				log.Debug("DuplicateTokenEx failed, lastError: " + errorMessage);
				return false;
			}
			CloseHandle(hProcess);
			CloseHandle(hPToken);
			return true;
		}

		[DllImport("userenv.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

		[DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
			ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
			string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurity]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

		[DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
		private static extern bool DuplicateTokenEx(IntPtr ExistingTokenHandle, uint dwDesiredAccess,
			ref SECURITY_ATTRIBUTES lpThreadAttributes, int TokenType,
			int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

		[DllImport("advapi32.dll", SetLastError = true, EntryPoint = "OpenProcessToken")]
		private static extern bool OpenProcessToken(SafeProcessHandle processHandle, UInt32 desiredAccess, out IntPtr tokenHandle);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "CloseHandle")]
		private static extern bool CloseHandle(IntPtr hObject);

		[DllImport("advapi32.dll", SetLastError = true, EntryPoint = "GetTokenInformation")]
		static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
			[MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
			ref TOKEN_PRIVILEGES NewState,
			int BufferLengthInBytes,
			ref TOKEN_PRIVILEGES PreviousState,
			out UInt32 ReturnLengthInBytes);

		[DllImport("advapi32.dll")]
		static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
			ref LUID lpLuid);

		private const int ANYSIZE_ARRAY = 1;

		[StructLayout(LayoutKind.Sequential)]
		struct TOKEN_PRIVILEGES
		{
			public int PrivilegeCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = ANYSIZE_ARRAY)]
			public LUID_AND_ATTRIBUTES[] Privileges;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct LUID_AND_ATTRIBUTES
		{
			public LUID Luid;
			public uint Attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct LUID
		{
			public uint LowPart;
			public uint HighPart;
		}

		private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
		private const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";

		private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
		private const int TOKEN_ASSIGN_PRIMARY = 0x1;
		private const int TOKEN_DUPLICATE = 0x2;
		private const int TOKEN_IMPERSONATE = 0x4;
		private const int TOKEN_QUERY = 0x8;
		private const int TOKEN_QUERY_SOURCE = 0x10;
		private const int TOKEN_ADJUST_GROUPS = 0x40;
		private const int TOKEN_ADJUST_PRIVILEGES = 0x20;
		private const int TOKEN_ADJUST_SESSIONID = 0x100;
		private const int TOKEN_ADJUST_DEFAULT = 0x80;
		private const int TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_SESSIONID | TOKEN_ADJUST_DEFAULT);

		/// <summary>
		/// Passed to <see cref="GetTokenInformation"/> to specify what
		/// information about the token to return.
		/// </summary>
		enum TokenInformationClass
		{
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics,
			TokenRestrictedSids,
			TokenSessionId,
			TokenGroupsAndPrivileges,
			TokenSessionReference,
			TokenSandBoxInert,
			TokenAuditPolicy,
			TokenOrigin,
			TokenElevationType,
			TokenLinkedToken,
			TokenElevation,
			TokenHasRestrictions,
			TokenAccessInformation,
			TokenVirtualizationAllowed,
			TokenVirtualizationEnabled,
			TokenIntegrityLevel,
			TokenUiAccess,
			TokenMandatoryPolicy,
			TokenLogonSid,
			MaxTokenInfoClass
		}

		/// <summary>
		/// The elevation type for a user token.
		/// </summary>
		enum TokenElevationType
		{
			TokenElevationTypeDefault = 1,
			TokenElevationTypeFull,
			TokenElevationTypeLimited
		}

		public struct TokenElevation
		{
			public UInt32 TokenIsElevated;
		}

		//http://msdn.microsoft.com/en-us/library/windows/desktop/ms684880(v=vs.85).aspx
		private enum ProcessAccessFlags
		{
			PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_ATTRIBUTES
		{
			public int Length;
			public IntPtr lpSecurityDescriptor;
			public bool bInheritHandle;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct STARTUPINFO
		{
			public int cb;
			public String lpReserved;
			public String lpDesktop;
			public String lpTitle;
			public uint dwX;
			public uint dwY;
			public uint dwXSize;
			public uint dwYSize;
			public uint dwXCountChars;
			public uint dwYCountChars;
			public uint dwFillAttribute;
			public uint dwFlags;
			public short wShowWindow;
			public short cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public uint dwProcessId;
			public uint dwThreadId;
		}

		#region Constants

		//public const int TOKEN_DUPLICATE = 0x0002;
		public const uint MAXIMUM_ALLOWED = 0x2000000;
		public const int CREATE_NEW_CONSOLE = 0x00000010;
		public const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;


		public const int IDLE_PRIORITY_CLASS = 0x40;
		public const int NORMAL_PRIORITY_CLASS = 0x20;
		public const int HIGH_PRIORITY_CLASS = 0x80;
		public const int REALTIME_PRIORITY_CLASS = 0x100;

		#endregion

		#region Enumerations

		enum TOKEN_TYPE : int
		{
			TokenPrimary = 1,
			TokenImpersonation = 2
		}

		enum SECURITY_IMPERSONATION_LEVEL : int
		{
			SecurityAnonymous = 0,
			SecurityIdentification = 1,
			SecurityImpersonation = 2,
			SecurityDelegation = 3,
		}

		#endregion

	}
}
