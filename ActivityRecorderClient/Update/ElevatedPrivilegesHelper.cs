using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Serialization;
using log4net;

namespace Tct.ActivityRecorderClient.Update
{
	public static class ElevatedPrivilegesHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static string FilePath { get { return "PrivSettings"; } }

		public static readonly bool IsElevated;
		public static readonly bool IsLocalAdmin;

		static ElevatedPrivilegesHelper()
		{
			IsElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
			//if (IsElevated) log.Info("Started with elevated privileges");//logged in ProgramWin - and we cannot call this from CO builds (because of the RunAsAdmin thing)
			try
			{
				if (ConfigManager.IsWindows7)
				{
					if (CheckIfAdminUser())
					{
						IsLocalAdmin = true;
						log.Info("User is a local administrator");
					}
				}
			}
			catch (PrincipalOperationException)
			{
				// group membership can't be determined, do nothing
			}
			if (IsolatedStorageSerializationHelper.Exists(FilePath) &&
				IsolatedStorageSerializationHelper.Load(FilePath, out runAsAdmin))
				log.Info("[RunAsAdmin] = '" + runAsAdmin + "'");
		}

		private static bool? runAsAdmin;

		public static bool RunAsAdmin
		{
			get
			{
				return (!ConfigManager.IsRunAsAdminDefault.HasValue || ConfigManager.IsRunAsAdminDefault.Value) && (runAsAdmin ?? ConfigManager.IsRunAsAdminDefault.HasValue && ConfigManager.IsRunAsAdminDefault.Value);
			}
			set
			{
				if (runAsAdmin == value) return;
				runAsAdmin = value;
				log.Info("[RunAsAdmin] = '" + value + "'");
				IsolatedStorageSerializationHelper.Save(FilePath, runAsAdmin);
			}
		}

		/// <summary>
		/// Checking current user admin role even if network is down or uac is enabled
		/// http://www.davidmoore.info/2011/06/20/how-to-check-if-the-current-user-is-an-administrator-even-if-uac-is-on/
		/// </summary>
		/// <returns>current user is in admin role</returns>
		private static bool CheckIfAdminUser()
		{
			var identity = WindowsIdentity.GetCurrent();
			if (identity == null) throw new InvalidOperationException("Couldn't get the current user identity");
			var principal = new WindowsPrincipal(identity);

			// Check if this user has the Administrator role. If they do, return immediately.
			// If UAC is on, and the process is not elevated, then this will actually return false.
			if (principal.IsInRole(WindowsBuiltInRole.Administrator)) return true;

			// If we're not running in Vista onwards, we don't have to worry about checking for UAC.
			if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6)
			{
				// Operating system does not support UAC; skipping elevation check.
				return false;
			}

			int tokenInfLength = Marshal.SizeOf(typeof(int));
			IntPtr tokenInformation = Marshal.AllocHGlobal(tokenInfLength);

			try
			{
				var token = identity.Token;
				var result = WinApi.GetTokenInformation(token, WinApi.TokenInformationClass.TokenElevationType, tokenInformation, tokenInfLength, out tokenInfLength);

				if (!result)
				{
					var exception = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
					throw new InvalidOperationException("Couldn't get token information", exception);
				}

				var elevationType = (WinApi.TokenElevationType)Marshal.ReadInt32(tokenInformation);

				switch (elevationType)
				{
					case WinApi.TokenElevationType.TokenElevationTypeDefault:
						// TokenElevationTypeDefault - User is not using a split token, so they cannot elevate.
						return false;
					case WinApi.TokenElevationType.TokenElevationTypeFull:
						// TokenElevationTypeFull - User has a split token, and the process is running elevated. Assuming they're an administrator.
						return true;
					case WinApi.TokenElevationType.TokenElevationTypeLimited:
						// TokenElevationTypeLimited - User has a split token, but the process is not running elevated. Assuming they're an administrator.
						return true;
					default:
						// Unknown token elevation type.
						return false;
				}
			}
			finally
			{
				if (tokenInformation != IntPtr.Zero) Marshal.FreeHGlobal(tokenInformation);
			}
		}

	}

}
