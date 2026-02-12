using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JC.Removal.Utils
{
	static class ElevatedHelper
	{
		internal static bool IsElevated;
		internal static bool IsUserLocalAdmin;

		static ElevatedHelper()
		{
			IsElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
			IsUserLocalAdmin = CheckIfAdminUser();
		}

		internal static void RestartAsAdmin()
		{
			var exeName = Process.GetCurrentProcess().MainModule?.FileName;
			if (exeName == null) return;
			ProcessStartInfo startInfo = new ProcessStartInfo(exeName) { Verb = "runas" };

			Process.Start(startInfo);
			Application.Exit();
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
				var result = GetTokenInformation(token, TokenInformationClass.TokenElevationType, tokenInformation, tokenInfLength, out tokenInfLength);

				if (!result)
				{
					var exception = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
					throw new InvalidOperationException("Couldn't get token information", exception);
				}

				var elevationType = (TokenElevationType)Marshal.ReadInt32(tokenInformation);

				switch (elevationType)
				{
					case TokenElevationType.TokenElevationTypeDefault:
						// TokenElevationTypeDefault - User is not using a split token, so they cannot elevate.
						return false;
					case TokenElevationType.TokenElevationTypeFull:
						// TokenElevationTypeFull - User has a split token, and the process is running elevated. Assuming they're an administrator.
						return true;
					case TokenElevationType.TokenElevationTypeLimited:
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

		[DllImport("advapi32.dll", SetLastError = true, EntryPoint = "GetTokenInformation")]
		private static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);
		/// <summary>
		/// Passed to <see cref="GetTokenInformation"/> to specify what
		/// information about the token to return.
		/// </summary>
		private enum TokenInformationClass
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
		private enum TokenElevationType
		{
			TokenElevationTypeDefault = 1,
			TokenElevationTypeFull,
			TokenElevationTypeLimited
		}
	}
}
